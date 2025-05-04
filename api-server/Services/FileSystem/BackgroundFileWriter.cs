using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Zarichney.Services.FileSystem;

public interface IFileWriteQueueService : IDisposable
{
  void QueueWrite(string directory, string filename, object data, string? extension = "json");
  Task WriteToFileAndWaitAsync(string directory, string filename, object data, string? extension = "json");
}

public class FileWriteQueueService : IFileWriteQueueService
{
  private readonly IFileService _fileService;
  private readonly ConcurrentQueue<WriteOperation> _writeQueue = new();
  private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingWrites = new();
  private readonly CancellationTokenSource _cancellationTokenSource = new();
  private readonly Task _processingTask;
  private readonly JsonSerializerOptions _jsonOptions;
  private readonly ILogger<FileWriteQueueService> _logger;

  public FileWriteQueueService(IFileService fileService, ILogger<FileWriteQueueService> logger)
  {
    _fileService = fileService;
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
    string filePath = GetFullPath(directory, filename, extension);
    var content = SerializeContent(data, extension);

    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    _pendingWrites[filePath] = tcs;

    _writeQueue.Enqueue(new WriteOperation(directory, filename, content, extension));
    _logger.LogInformation("Queued file write: {FilePath}", filePath);
  }

  public async Task WriteToFileAndWaitAsync(string directory, string filename, object data, string? extension = "json")
  {
    extension ??= "json";
    string filePath = GetFullPath(directory, filename, extension);
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
        string filePath = GetFullPath(op.Directory, op.Filename, op.Extension);
        try
        {
          await _fileService.CreateFile(filePath, op.Data, MimeTypeFor(op.Extension));
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Write failed for file: {FilePath}", filePath);
        }
        finally
        {
          if (_pendingWrites.TryRemove(filePath, out var tcs))
          {
            tcs.TrySetResult(true);
          }
        }
      }
      else
      {
        await Task.Delay(100);
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
      "json" => JsonSerializer.Serialize(data, _jsonOptions),
      "md" or "txt" => data.ToString()!,
      "pdf" => data is byte[] bytes ? bytes : throw new ArgumentException("PDF content must be byte array"),
      _ => throw new ArgumentException($"Unsupported extension: {extension}")
    };
  }

  public void Dispose()
  {
    _cancellationTokenSource.Cancel();
    try
    {
      _processingTask.Wait();
    }
    catch
    {
      // ignored
    }

    _cancellationTokenSource.Dispose();
  }

  private record WriteOperation(string Directory, string Filename, object Data, string Extension);
}
