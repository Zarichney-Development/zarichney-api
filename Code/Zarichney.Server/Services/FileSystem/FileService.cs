using System.Text.Encodings.Web;
using System.Text.Json;
using Polly;
using Polly.Retry;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Zarichney.Services.FileSystem;

public interface IFileService
{
  Task<T?> ReadFromFile<T>(string directory, string filename, string? extension = "json");
  string[] GetFiles(string directoryPath);
  string GetFile(string filePath);
  Task<string> GetFileAsync(string filePath);
  Task CreateFile(string filePath, object data, string fileType);
  void DeleteFile(string filePath);
  bool FileExists(string? filePath);
}

public class FileService(ILogger<FileService> logger) : IFileService
{
  private readonly AsyncRetryPolicy _retryPolicy = Policy
      .Handle<Exception>()
      .WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(200),
          (ex, _, retryCount, ctx) =>
          {
            logger.LogWarning(ex, "Read attempt {RetryCount}: Retrying due to {Message}. Context: {@Context}",
            retryCount, ex.Message, ctx);
          });

  private readonly JsonSerializerOptions _jsonOptions = new()
  {
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
  };

  public async Task<T?> ReadFromFile<T>(string directory, string filename, string? extension = "json")
  {
    extension ??= "json";
    var filePath = GetFullPath(directory, filename, extension);
    logger.LogInformation("Reading file: {FilePath}", filePath);

    var data = await LoadExistingData(filePath, extension);
    if (data == null) return default;

    if (extension.Equals("pdf", StringComparison.OrdinalIgnoreCase))
      return (T)data;

    if (data is not JsonElement jsonElement)
      return (T)data;

    return Utils.Deserialize<T>(jsonElement.GetRawText())!;
  }

  public string[] GetFiles(string directoryPath)
  {
    if (!Directory.Exists(directoryPath))
      Directory.CreateDirectory(directoryPath);

    return Directory.GetFiles(directoryPath);
  }

  public string GetFile(string filePath)
    => File.ReadAllText(filePath);

  public async Task<string> GetFileAsync(string filePath)
    => await File.ReadAllTextAsync(filePath);

  public async Task CreateFile(string filePath, object data, string fileType)
  {
    switch (fileType.ToLower())
    {
      case "image/jpeg":
        await using (var stream = File.Create(filePath))
        {
          await ((Image)data).SaveAsJpegAsync(stream, new JpegEncoder { Quality = 90 });
        }

        logger.LogInformation("Created JPEG file: {FilePath}", filePath);
        break;
      case "application/pdf":
        await File.WriteAllBytesAsync(filePath, (byte[])data);
        logger.LogInformation("Created PDF file: {FilePath}", filePath);
        break;
      case "application/json":
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(data, _jsonOptions));
        logger.LogInformation("Created JSON file: {FilePath}", filePath);
        break;
      case "text/plain":
        await File.WriteAllTextAsync(filePath, data.ToString());
        logger.LogInformation("Created Text file: {FilePath}", filePath);
        break;
      default:
        throw new ArgumentException($"Unsupported file type: {fileType}");
    }
  }

  public void DeleteFile(string filePath)
  {
    // If the file does not exist, treat as a no-op for safe idempotency
    if (!File.Exists(filePath))
    {
      logger.LogInformation("Delete requested for non-existent file: {FilePath}", filePath);
      return;
    }

    var retryPolicy = Policy
      .Handle<IOException>()
      .Or<UnauthorizedAccessException>()
      .WaitAndRetry(3, _ => TimeSpan.FromMilliseconds(200),
        (ex, ts, retryCount, _) =>
        {
          logger.LogWarning(ex, "Retry {RetryCount}: Unable to delete file: {FilePath}. Retrying in {Delay}ms",
            retryCount, filePath, ts.TotalMilliseconds);
        });

    retryPolicy.Execute(() =>
    {
      if (IsFileLocked(filePath))
        throw new UnauthorizedAccessException();

      File.Delete(filePath);
      logger.LogInformation("Deleted file: {FilePath}", filePath);
    });
  }

  public bool FileExists(string? filePath)
    => !string.IsNullOrEmpty(filePath) && File.Exists(filePath);

  private async Task<object?> LoadExistingData(string filePath, string extension)
  {
    if (!File.Exists(filePath)) return null;

    return await _retryPolicy.ExecuteAsync(async () =>
    {
      return extension.ToLower() switch
      {
        "json" => Utils.Deserialize<object>(await GetFileAsync(filePath)),
        "md" or "txt" => await GetFileAsync(filePath),
        "pdf" => await File.ReadAllBytesAsync(filePath),
        _ => throw new ArgumentException($"Unsupported extension: {extension}")
      };
    });
  }

  private bool IsFileLocked(string filePath)
  {
    try
    {
      using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
      return false;
    }
    catch (IOException)
    {
      return true;
    }
  }

  private static string GetFullPath(string directory, string filename, string extension)
    => Path.Combine(directory, $"{SanitizeFileName(filename)}.{extension}");

  public static string SanitizeFileName(string fileName)
  {
    if (string.IsNullOrEmpty(fileName)) return string.Empty;

    // Cross-platform invalid set (Windows + common unsafe chars), merged with OS-specific invalids
    ReadOnlySpan<char> commonInvalid = new[] { '/', '\\', '<', '>', ':', '"', '|', '?', '*' };
    var osInvalid = Path.GetInvalidFileNameChars();
    HashSet<char> allInvalid = [.. osInvalid];
    foreach (var c in commonInvalid) allInvalid.Add(c);

    // Split on invalid characters and join with single underscores to collapse runs
    var sanitized = string.Join("_", fileName.Split(allInvalid.ToArray(), StringSplitOptions.RemoveEmptyEntries));
    return sanitized;
  }
}
