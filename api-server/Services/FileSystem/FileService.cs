using System.Text.Encodings.Web;
using System.Text.Json;
using Polly;
using Polly.Retry;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Zarichney.Services.FileSystem;

public interface IFileService
{
  Task WriteToFileAndWait(string directory, string filename, object data, string? extension = "json");
  void QueueWrite(string directory, string filename, object data, string? extension = "json");
  Task<T> ReadFromFile<T>(string directory, string filename, string? extension = "json");
  string[] GetFiles(string directoryPath);
  string GetFile(string filePath);
  Task<string> GetFileAsync(string filePath);
  Task CreateFile(string filePath, object data, string fileType);
  void DeleteFile(string filePath);
  bool FileExists(string? filePath);
}

public class FileService : IFileService
{
  private readonly AsyncRetryPolicy _retryPolicy;
  private readonly JsonSerializerOptions _jsonOptions;
  private readonly ILogger<FileService> _logger;
  private readonly IFileWriteQueueService _writeQueueService;

  public FileService(IFileWriteQueueService writeQueueService, ILogger<FileService> logger)
  {
    _writeQueueService = writeQueueService;
    _logger = logger;
    _jsonOptions = new JsonSerializerOptions
    {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    _retryPolicy = Policy
      .Handle<Exception>()
      .WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(200),
        (ex, _, retryCount, ctx) =>
        {
          _logger.LogWarning(ex, "Read attempt {RetryCount}: Retrying due to {Message}. Context: {@Context}",
            retryCount, ex.Message, ctx);
        });
  }

  public async Task WriteToFileAndWait(string directory, string filename, object data, string? extension = "json")
    => await _writeQueueService.WriteToFileAndWaitAsync(directory, filename, data, extension);

  public void QueueWrite(string directory, string filename, object data, string? extension = "json")
    => _writeQueueService.QueueWrite(directory, filename, data, extension);

  public async Task<T> ReadFromFile<T>(string directory, string filename, string? extension = "json")
  {
    extension ??= "json";
    var filePath = GetFullPath(directory, filename, extension);
    _logger.LogInformation("Reading file: {FilePath}", filePath);

    var data = await LoadExistingData(filePath, extension);
    if (data == null) return default!;

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
    => GetFileAsync(filePath).GetAwaiter().GetResult();

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

        _logger.LogInformation("Created JPEG file: {FilePath}", filePath);
        break;
      case "application/pdf":
        await File.WriteAllBytesAsync(filePath, (byte[])data);
        _logger.LogInformation("Created PDF file: {FilePath}", filePath);
        break;
      case "application/json":
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(data, _jsonOptions));
        _logger.LogInformation("Created JSON file: {FilePath}", filePath);
        break;
      case "text/plain":
        await File.WriteAllTextAsync(filePath, data.ToString());
        _logger.LogInformation("Created Text file: {FilePath}", filePath);
        break;
      default:
        throw new ArgumentException($"Unsupported file type: {fileType}");
    }
  }

  public void DeleteFile(string filePath)
  {
    var retryPolicy = Policy
      .Handle<IOException>()
      .Or<UnauthorizedAccessException>()
      .WaitAndRetry(3, _ => TimeSpan.FromMilliseconds(200),
        (ex, ts, retryCount, _) =>
        {
          _logger.LogWarning(ex, "Retry {RetryCount}: Unable to delete file: {FilePath}. Retrying in {Delay}ms",
            retryCount, filePath, ts.TotalMilliseconds);
        });

    retryPolicy.Execute(() =>
    {
      if (IsFileLocked(filePath))
        throw new UnauthorizedAccessException();

      File.Delete(filePath);
      _logger.LogInformation("Deleted file: {FilePath}", filePath);
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
    var invalidChars = Path.GetInvalidFileNameChars();
    return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
  }
}