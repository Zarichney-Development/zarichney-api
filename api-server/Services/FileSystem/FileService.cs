using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Zarichney.Services.FileSystem;

/// <summary>
/// Provides file read/write operations with concurrency handling.
/// </summary>
public interface IFileService : IDisposable
{
  Task WriteToFile(string directory, string filename, object data, string? extension = "json");
  void WriteToFileAsync(string directory, string filename, object data, string? extension = "json");
  Task<T> ReadFromFile<T>(string directory, string filename, string? extension = "json");
  string[] GetFiles(string directoryPath);
  string GetFile(string filePath);
  Task<string> GetFileAsync(string filePath);
  Task CreateFile(string filePath, object data, string fileType);
  void DeleteFile(string filePath);
  bool FileExists(string? filePath);
}

/// <summary>
/// Concrete implementation of <see cref="IFileService"/> for handling file operations.
/// Uses a queue-based concurrency model for write operations.
/// </summary>
public class FileService : IFileService
{
  #region Private Fields

  // Queue for pending write operations to be processed serially
  private readonly ConcurrentQueue<WriteOperation> _writeQueue = new();

  // Task that continuously processes write operations from the queue
  private readonly Task _processQueueTask;

  // Token used to request cancellation of the background queue processing
  private readonly CancellationTokenSource _cancellationTokenSource = new();

  // Tracks write operations in progress, ensuring reads wait for them to complete
  private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingWrites = new();

  // Retry policy (Polly) for handling transient exceptions on read operations
  private readonly AsyncRetryPolicy _retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(
      retryCount: 5,
      sleepDurationProvider: _ => TimeSpan.FromMilliseconds(200),
      onRetry: (exception, _, retryCount, context) =>
      {
        Log.Warning(exception,
          "Read attempt {retryCount}: Retrying due to {exception}. Retry Context: {@Context}",
          retryCount, exception.Message, context);
      }
    );

  // JSON serialization options
  private readonly JsonSerializerOptions _jsonSerializerOptions = new()
  {
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
  };

  private readonly ILogger<FileService> _logger;

  #endregion

  #region Constructor

  /// <summary>
  /// Initializes the file service and starts the background process queue task.
  /// </summary>
  public FileService(ILogger<FileService> logger)
  {
    _logger = logger;
    // Launch a background task to process any queued write operations
    _processQueueTask = Task.Run(ProcessQueueAsync);
  }

  #endregion

  #region Public Write Methods

  /// <summary>
  /// Writes data to a file synchronously, waiting for the operation to complete.
  /// Supports JSON, markdown/txt, and PDF formats.
  /// </summary>
  /// <param name="directory">The directory path.</param>
  /// <param name="filename">The file name without extension.</param>
  /// <param name="data">The data to write.</param>
  /// <param name="extension">The file extension to use (default is "json").</param>
  public async Task WriteToFile(string directory, string filename, object data, string? extension = null)
  {
    extension ??= "json";
    _logger.LogInformation("Writing file: {Filename}", filename);
    object content;
    switch (extension.ToLower())
    {
      case "json":
        content = JsonSerializer.Serialize(data, _jsonSerializerOptions);
        break;
      case "md":
      case "txt":
        content = data.ToString()!;
        break;
      case "pdf":
        if (data is not byte[] pdfData)
          throw new ArgumentException("PDF data must be provided as a byte array");
        content = pdfData;
        break;
      default:
        throw new ArgumentException($"Unsupported file extension: {extension}");
    }

    var writeOperation = new WriteOperation(directory, filename, content, extension);
    var filePath = GetFullPath(directory, filename, extension);

    // Create a TaskCompletionSource to track completion for this file path
    var tcs = new TaskCompletionSource<bool>();
    _pendingWrites.TryAdd(filePath, tcs);

    // Perform the write immediately on this thread
    await PerformWriteOperationAsync(writeOperation);

    // Mark the write as completed
    if (_pendingWrites.TryRemove(filePath, out var completionSource))
    {
      completionSource.SetResult(true);
    }
  }

  /// <summary>
  /// Enqueues a write operation to be performed asynchronously by the background queue.
  /// Returns immediately without waiting for the operation to complete.
  /// </summary>
  /// <param name="directory">The directory path.</param>
  /// <param name="filename">The file name without extension.</param>
  /// <param name="data">The data to write.</param>
  /// <param name="extension">The file extension to use (default is "json").</param>
  public void WriteToFileAsync(string directory, string filename, object data, string? extension = null)
  {
    extension ??= "json";
    _logger.LogInformation("Writing file: {Filename}", filename);
    object content;
    switch (extension.ToLower())
    {
      case "json":
        content = JsonSerializer.Serialize(data, _jsonSerializerOptions);
        break;
      case "md":
      case "txt":
        content = data.ToString()!;
        break;
      case "pdf":
        if (data is not byte[] pdfData)
          throw new ArgumentException("PDF data must be provided as a byte array");
        content = pdfData;
        break;
      default:
        throw new ArgumentException($"Unsupported file extension: {extension}");
    }

    var filePath = GetFullPath(directory, filename, extension);

    // Create a TaskCompletionSource to track completion for this file path
    var tcs = new TaskCompletionSource<bool>();
    _pendingWrites.TryAdd(filePath, tcs);

    // Enqueue the write operation for the background task to process
    _writeQueue.Enqueue(new WriteOperation(directory, filename, content, extension));
  }

  #endregion

  #region Public Read Methods

  /// <summary>
  /// Reads and deserializes data from a file of type T.
  /// Waits if a pending write operation is still in progress for the target file.
  /// </summary>
  /// <typeparam name="T">Type to which JSON data will be deserialized.</typeparam>
  /// <param name="directory">The directory path.</param>
  /// <param name="filename">The file name without extension.</param>
  /// <param name="extension">The file extension (default is "json").</param>
  /// <returns>An instance of T containing the deserialized data.</returns>
  public async Task<T> ReadFromFile<T>(string directory, string filename, string? extension = null)
  {
    extension ??= "json";
    var filePath = GetFullPath(directory, filename, extension);

    // If there's write-in progress for this file, wait for it
    if (_pendingWrites.TryGetValue(filePath, out var pendingWrite))
    {
      _logger.LogInformation("Waiting for pending write operation to complete for file: {FilePath}", filePath);
      await pendingWrite.Task;
    }

    var data = await LoadExistingData(directory, filename, extension);

    if (data == null)
    {
      return default!;
    }

    // PDF data is returned as a byte[] directly
    if (extension.Equals("pdf", StringComparison.CurrentCultureIgnoreCase))
    {
      return (T)data;
    }

    // Non-JSON data might be a simple string
    if (data is not JsonElement jsonElement)
    {
      return (T)data;
    }

    // If it's JSON data, deserialize
    return Utils.Deserialize<T>(jsonElement.GetRawText())!;
  }

  /// <summary>
  /// Returns the content of a file as a string (blocking call).
  /// </summary>
  /// <param name="filePath">The full path of the file.</param>
  /// <returns>A string containing the file content.</returns>
  public string GetFile(string filePath)
  {
    var task = GetFileAsync(filePath);
    task.Wait();
    return task.Result;
  }

  /// <summary>
  /// Returns the content of a file as a string (asynchronous).
  /// </summary>
  /// <param name="filePath">The full path of the file.</param>
  /// <returns>A Task containing the file content as string.</returns>
  public async Task<string> GetFileAsync(string filePath)
  {
    return await File.ReadAllTextAsync(filePath);
  }

  /// <summary>
  /// Returns the content of a file as a byte array (asynchronous).
  /// </summary>
  /// <param name="filePath">The full path of the file.</param>
  /// <returns>A Task containing the file content as a byte array.</returns>
  private async Task<byte[]> GetFileBytes(string filePath)
  {
    return await File.ReadAllBytesAsync(filePath);
  }

  #endregion

  #region Public File Management Methods

  /// <summary>
  /// Returns an array of file paths in the specified directory. Creates the directory if it does not exist.
  /// </summary>
  /// <param name="directoryPath">The directory path.</param>
  /// <returns>An array of file paths.</returns>
  public string[] GetFiles(string directoryPath)
  {
    if (!Directory.Exists(directoryPath))
    {
      Directory.CreateDirectory(directoryPath);
    }
    return Directory.GetFiles(directoryPath);
  }

  /// <summary>
  /// Creates a file with the specified data and file type (e.g., image/jpeg).
  /// </summary>
  /// <param name="filePath">The full path where the file will be created.</param>
  /// <param name="data">The data used to create the file (e.g., <see cref="Image"/> for JPEG).</param>
  /// <param name="fileType">The file type (e.g., "image/jpeg").</param>
  public async Task CreateFile(string filePath, object data, string fileType)
  {
    switch (fileType.ToLower())
    {
      case "image/jpeg":
        await using (var fileStream = File.Create(filePath))
        {
          await ((Image?)data).SaveAsJpegAsync(fileStream, new JpegEncoder { Quality = 90 });
          _logger.LogInformation("Created JPEG file: {FilePath}", filePath);
        }
        return;

      default:
        throw new ArgumentException($"Unsupported file type: {fileType}");
    }
  }

  /// <summary>
  /// Deletes a file using a retry policy to handle locked or unauthorized access scenarios.
  /// </summary>
  /// <param name="filePath">The full path of the file to delete.</param>
  public void DeleteFile(string filePath)
  {
    var retryPolicy = Policy
      .Handle<IOException>()
      .Or<UnauthorizedAccessException>()
      .WaitAndRetry(
        retryCount: 3,
        sleepDurationProvider: _ => TimeSpan.FromMilliseconds(200),
        onRetry: (exception, timeSpan, retryCount, _) =>
        {
          _logger.LogWarning(exception, "Retry {RetryCount}: Unable to delete file: {FilePath}. Retrying in {RetryTime}ms",
            retryCount, filePath, timeSpan.TotalMilliseconds);
        });

    retryPolicy.Execute(() =>
    {
      if (IsFileLocked(filePath))
      {
        _logger.LogWarning("File is locked: {FilePath}", filePath);
        throw new UnauthorizedAccessException();
      }

      File.Delete(filePath);
      _logger.LogInformation("Deleted file: {FilePath}", filePath);
    });
  }

  /// <summary>
  /// Checks if a file exists at the given path.
  /// </summary>
  /// <param name="filePath">The file path to check.</param>
  /// <returns>True if file exists, otherwise false.</returns>
  public bool FileExists(string? filePath)
    => !string.IsNullOrEmpty(filePath) && File.Exists(filePath);

  #endregion

  #region IDisposable Implementation

  /// <summary>
  /// Cleans up the background queue processing by canceling and waiting for any remaining operations.
  /// </summary>
  public void Dispose()
  {
    _cancellationTokenSource.Cancel();
    _processQueueTask.Wait();
    _cancellationTokenSource.Dispose();

    // Cancel any pending write operations
    foreach (var pendingWrite in _pendingWrites.Values)
    {
      pendingWrite.TrySetCanceled();
    }
  }

  #endregion

  #region Private/Internal Helpers

  /// <summary>
  /// Continuously processes queued write operations until cancellation is requested.
  /// </summary>
  private async Task ProcessQueueAsync()
  {
    while (!_cancellationTokenSource.Token.IsCancellationRequested)
    {
      if (_writeQueue.TryDequeue(out var operation))
      {
        var filePath = GetFullPath(operation.Directory, operation.Filename, operation.Extension);
        try
        {
          await PerformWriteOperationAsync(operation);
        }
        finally
        {
          // Signal that the write operation is complete
          if (_pendingWrites.TryRemove(filePath, out var completionSource))
          {
            completionSource.SetResult(true);
          }
        }
      }
      else
      {
        // No operations in the queue, wait briefly before checking again
        await Task.Delay(100);
      }
    }
  }

  /// <summary>
  /// Handles the actual writing of file data, supporting different file formats.
  /// </summary>
  /// <param name="operation">A <see cref="WriteOperation"/> describing the write request.</param>
  private async Task PerformWriteOperationAsync(WriteOperation operation)
  {
    try
    {
      // Ensure the directory exists
      if (!Directory.Exists(operation.Directory))
      {
        Directory.CreateDirectory(operation.Directory);
      }

      var fileNamePath = Path.Combine(operation.Directory,
        $"{SanitizeFileName(operation.Filename)}.{operation.Extension}");

      // If it's PDF data, write as bytes; otherwise, write as text
      if (operation.Data is byte[] pdfData)
      {
        await File.WriteAllBytesAsync(fileNamePath, pdfData);
      }
      else
      {
        await using var fileStream = new FileStream(fileNamePath, FileMode.Create, FileAccess.Write,
          FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
        await using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        await streamWriter.WriteAsync(operation.Data.ToString());
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error writing file: {Filename}", operation.Filename);
    }
  }

  /// <summary>
  /// Loads existing file data, applying a retry policy for transient errors.
  /// </summary>
  /// <param name="directory">The directory path.</param>
  /// <param name="filename">The file name without extension.</param>
  /// <param name="extension">The file extension.</param>
  /// <returns>An <see cref="object"/> representing the file contents.</returns>
  private async Task<object?> LoadExistingData(string directory, string filename, string? extension = null)
  {
    extension ??= "json";
    var filePath = Path.Combine(directory, $"{SanitizeFileName(filename)}.{extension}");

    _logger.LogInformation("Loading existing data from '{FilePath}'", filePath);

    if (!File.Exists(filePath)) return null;

    try
    {
      return await _retryPolicy.ExecuteAsync(async () =>
      {
        switch (extension.ToLower())
        {
          case "json":
            var jsonContent = await GetFileAsync(filePath);
            return Utils.Deserialize<object>(jsonContent);
          case "md":
          case "txt":
            return await GetFileAsync(filePath);
          case "pdf":
            return await GetFileBytes(filePath);
          default:
            throw new ArgumentException($"Unsupported file extension: {extension}");
        }
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, $"Error loading existing data from '{filePath}'");
      return null;
    }
  }

  /// <summary>
  /// Determines if a file is currently locked by another process.
  /// </summary>
  /// <param name="filePath">The full file path to check.</param>
  /// <returns>True if the file is locked; otherwise, false.</returns>
  private bool IsFileLocked(string filePath)
  {
    try
    {
      using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
      return false; // File is not locked
    }
    catch (IOException)
    {
      return true; // File is locked
    }
  }

  /// <summary>
  /// Builds a valid file path by combining directory, filename, and extension into a single path.
  /// </summary>
  private static string GetFullPath(string directory, string filename, string? extension)
    => Path.Combine(directory, $"{SanitizeFileName(filename)}.{extension}");

  /// <summary>
  /// Sanitizes a file name by replacing invalid characters and spaces with underscores.
  /// </summary>
  internal static string SanitizeFileName(string fileName)
  {
    var invalidChars = new List<char> { ' ', '-' };
    invalidChars.AddRange(Path.GetInvalidFileNameChars());
    return string.Join("_", fileName.Split(invalidChars.ToArray(), StringSplitOptions.RemoveEmptyEntries));
  }

  #endregion
}

/// <summary>
/// Represents a pending write operation with directory, filename, and data.
/// </summary>
public class WriteOperation(string directory, string filename, object data, string? extension)
{
  public string Directory { get; } = directory;
  public string Filename { get; } = filename;
  public object Data { get; } = data;
  public string? Extension { get; } = extension;
}