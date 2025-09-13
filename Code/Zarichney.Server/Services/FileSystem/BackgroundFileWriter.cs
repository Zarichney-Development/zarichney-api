using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Zarichney.Services.FileSystem;

public interface IFileWriteQueueService : IDisposable, IAsyncDisposable
{
  void QueueWrite(string directory, string filename, object data, string? extension = "json");
  Task WriteToFileAndWaitAsync(string directory, string filename, object data, string? extension = "json");
}

internal class FileWriteQueueService : IFileWriteQueueService
{
  private readonly ConcurrentQueue<WriteOperation> _writeQueue = new();
  private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingWrites = new();
  private readonly CancellationTokenSource _cancellationTokenSource = new();
  private readonly Task _processingTask;
  private readonly JsonSerializerOptions _jsonOptions;
  private readonly ILogger<FileWriteQueueService> _logger;

  public FileWriteQueueService(ILogger<FileWriteQueueService> logger)
  {
    _logger = logger;
    _jsonOptions = new JsonSerializerOptions
    {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    _processingTask = Task.Run(ProcessQueueAsync);
  }

  public void QueueWrite(string directory, string filename, object data, string? extension = "json")
  {
    extension ??= "json";
    var filePath = GetFullPath(directory, filename, extension);
    var content = SerializeContent(data, extension);

    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    _pendingWrites[filePath] = tcs;

    _writeQueue.Enqueue(new WriteOperation(directory, filename, content, extension));
    _logger.LogInformation("Queued file write: {FilePath}", filePath);
  }

  public async Task WriteToFileAndWaitAsync(string directory, string filename, object data, string? extension = "json")
  {
    extension ??= "json";
    var filePath = GetFullPath(directory, filename, extension);
    var content = SerializeContent(data, extension);

    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    _pendingWrites[filePath] = tcs;

    _writeQueue.Enqueue(new WriteOperation(directory, filename, content, extension));
    _logger.LogInformation("Queued file write with await: {FilePath}", filePath);

    await tcs.Task;
  }

  private async Task ProcessQueueAsync()
  {
    while (!_cancellationTokenSource.IsCancellationRequested)
    {
      if (_writeQueue.TryDequeue(out var op))
      {
        var filePath = GetFullPath(op.Directory, op.Filename, op.Extension);
        try
        {
          Directory.CreateDirectory(op.Directory);

          var mimeType = MimeTypeFor(op.Extension);
          switch (mimeType.ToLower())
          {
            case "application/pdf":
              await File.WriteAllBytesAsync(filePath, (byte[])op.Data, _cancellationTokenSource.Token);
              _logger.LogInformation("BackgroundWriter created PDF file: {FilePath}", filePath);
              break;
            case "application/json":
              await File.WriteAllTextAsync(filePath, (string)op.Data, _cancellationTokenSource.Token);
              _logger.LogInformation("BackgroundWriter created JSON file: {FilePath}", filePath);
              break;
            case "text/plain":
              await File.WriteAllTextAsync(filePath, (string)op.Data, _cancellationTokenSource.Token);
              _logger.LogInformation("BackgroundWriter created Text file: {FilePath}", filePath);
              break;
            default:
              _logger.LogWarning("BackgroundWriter encountered unsupported file type via extension '{Extension}' for file: {FilePath}", op.Extension, filePath);
              break;
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "BackgroundWriter failed for file: {FilePath}", filePath);
          if (_pendingWrites.TryRemove(filePath, out var failedTcs))
          {
            failedTcs.TrySetException(ex);
          }
        }
        finally
        {
          if (_pendingWrites.TryRemove(filePath, out var successTcs) && !successTcs.Task.IsCompleted)
          {
            successTcs.TrySetResult(true);
          }
        }
      }
      else
      {
        await Task.Delay(100, _cancellationTokenSource.Token);
      }
    }
  }

  private static string GetFullPath(string directory, string filename, string extension)
    => Path.Combine(directory, $"{SanitizeFileName(filename)}.{extension}");

  private static string SanitizeFileName(string fileName)
    => string.Join("_", fileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

  private static string MimeTypeFor(string extension) => extension.ToLower() switch
  {
    "pdf" => "application/pdf",
    "jpg" or "jpeg" => "image/jpeg",
    "json" => "application/json",
    _ => "text/plain"
  };

  private object SerializeContent(object data, string extension)
  {
    return extension.ToLower() switch
    {
      "json" => data is string s ? s : JsonSerializer.Serialize(data, _jsonOptions),
      "md" or "txt" => data.ToString()!,
      "pdf" => data is byte[] bytes ? bytes : throw new ArgumentException("PDF content must be byte array"),
      _ => throw new ArgumentException($"Unsupported extension for serialization: {extension}")
    };
  }

  void IDisposable.Dispose()
  {
    DisposeAsync().AsTask().Wait();
  }

  public async ValueTask DisposeAsync()
  {
    _cancellationTokenSource.Cancel();
    try
    {
      await _processingTask;
    }
    catch
    {
      // ignored
    }

    _cancellationTokenSource.Dispose();
  }

  private record WriteOperation(string Directory, string Filename, object Data, string Extension);
}
