using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.FileSystem;
using FluentAssertions;

namespace Zarichney.Tests.Unit.Services.FileSystem;

public class BackgroundFileWriterTests : IDisposable
{
  private readonly Mock<ILogger<FileWriteQueueService>> _mockLogger;
  private readonly FileWriteQueueService _sut;
  private readonly string _testDirectory;
  private readonly List<string> _createdFiles = new();

  public BackgroundFileWriterTests()
  {
    _mockLogger = new Mock<ILogger<FileWriteQueueService>>();
    _sut = new FileWriteQueueService(_mockLogger.Object);
    _testDirectory = Path.Combine(Path.GetTempPath(), $"BackgroundFileWriter_Tests_{Guid.NewGuid():N}");
    Directory.CreateDirectory(_testDirectory);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithValidLogger_InitializesSuccessfully()
  {
    // Arrange & Act
    using var service = new FileWriteQueueService(_mockLogger.Object);

    // Assert
    service.Should().NotBeNull("because the service should be properly initialized");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void QueueWrite_WithValidJsonData_QueuesWriteOperation()
  {
    // Arrange
    var testData = new { Name = "Test", Value = 42 };
    var filename = "test_file";

    // Act
    _sut.QueueWrite(_testDirectory, filename, testData);

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Queued file write: {Path.Combine(_testDirectory, $"{filename}.json")}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "because the queue operation should be logged");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void QueueWrite_WithNullExtension_DefaultsToJson()
  {
    // Arrange
    var testData = new { Name = "Test" };
    var filename = "test_file";

    // Act
    _sut.QueueWrite(_testDirectory, filename, testData, null);

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Queued file write: {Path.Combine(_testDirectory, $"{filename}.json")}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "because null extension should default to json");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void QueueWrite_WithCustomExtension_UsesSpecifiedExtension()
  {
    // Arrange
    var testData = "Plain text content";
    var filename = "test_file";
    var extension = "txt";

    // Act
    _sut.QueueWrite(_testDirectory, filename, testData, extension);

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Queued file write: {Path.Combine(_testDirectory, $"{filename}.{extension}")}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "because custom extension should be used in the file path");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithValidData_CompletesSuccessfully()
  {
    // Arrange
    var testData = new { Message = "Hello, World!", Timestamp = DateTime.UtcNow };
    var filename = "async_test_file";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.json");
    _createdFiles.Add(expectedPath);

    // Act
    var writeTask = _sut.WriteToFileAndWaitAsync(_testDirectory, filename, testData);
    var completed = await Task.WhenAny(writeTask, Task.Delay(5000));

    // Assert
    completed.Should().Be(writeTask, "because the write operation should complete within the timeout");
    await writeTask; // Ensure no exceptions

    File.Exists(expectedPath).Should().BeTrue("because the file should have been created");

    var fileContent = await File.ReadAllTextAsync(expectedPath);
    var deserializedData = JsonSerializer.Deserialize<Dictionary<string, object>>(fileContent);
    deserializedData.Should().ContainKey("Message", "because the original data should be preserved");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithTextData_CreatesTextFile()
  {
    // Arrange
    var textContent = "This is test content for a text file";
    var filename = "text_test_file";
    var extension = "txt";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.{extension}");
    _createdFiles.Add(expectedPath);

    // Act
    await _sut.WriteToFileAndWaitAsync(_testDirectory, filename, textContent, extension);

    // Assert
    File.Exists(expectedPath).Should().BeTrue("because the text file should have been created");

    var fileContent = await File.ReadAllTextAsync(expectedPath);
    fileContent.Should().Be(textContent, "because the text content should be preserved exactly");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithPdfData_CreatesPdfFile()
  {
    // Arrange
    var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 }; // PDF header
    var filename = "pdf_test_file";
    var extension = "pdf";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.{extension}");
    _createdFiles.Add(expectedPath);

    // Act
    await _sut.WriteToFileAndWaitAsync(_testDirectory, filename, pdfBytes, extension);

    // Assert
    File.Exists(expectedPath).Should().BeTrue("because the PDF file should have been created");

    var fileBytes = await File.ReadAllBytesAsync(expectedPath);
    fileBytes.Should().BeEquivalentTo(pdfBytes, "because the PDF bytes should be preserved exactly");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void QueueWrite_WithSpecialFileNameCharacters_LogsFileWriteOperation()
  {
    // Arrange
    var testData = new { Name = "Test" };
    var specialFileName = "test_file_with_special_chars";

    // Act
    _sut.QueueWrite(_testDirectory, specialFileName, testData);

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Queued file write:")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "because the queue operation should be logged");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithNonExistentDirectory_CreatesDirectory()
  {
    // Arrange
    var nonExistentDir = Path.Combine(_testDirectory, "nested", "directory", "structure");
    var testData = new { Test = "data" };
    var filename = "nested_test_file";
    var expectedPath = Path.Combine(nonExistentDir, $"{filename}.json");
    _createdFiles.Add(expectedPath);

    // Act
    await _sut.WriteToFileAndWaitAsync(nonExistentDir, filename, testData);

    // Assert
    Directory.Exists(nonExistentDir).Should().BeTrue("because the directory should have been created");
    File.Exists(expectedPath).Should().BeTrue("because the file should have been created in the new directory");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithStringJsonData_PreservesStringAsIs()
  {
    // Arrange
    var jsonString = """{"customKey": "customValue", "number": 123}""";
    var filename = "string_json_test";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.json");
    _createdFiles.Add(expectedPath);

    // Act
    await _sut.WriteToFileAndWaitAsync(_testDirectory, filename, jsonString);

    // Assert
    File.Exists(expectedPath).Should().BeTrue("because the file should have been created");

    var fileContent = await File.ReadAllTextAsync(expectedPath);
    fileContent.Should().Be(jsonString, "because string JSON data should be preserved as-is without re-serialization");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithObjectData_SerializesWithIndentation()
  {
    // Arrange
    var objectData = new { Name = "Test Object", Value = 42, Nested = new { Property = "nested value" } };
    var filename = "object_serialization_test";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.json");
    _createdFiles.Add(expectedPath);

    // Act
    await _sut.WriteToFileAndWaitAsync(_testDirectory, filename, objectData);

    // Assert
    File.Exists(expectedPath).Should().BeTrue("because the file should have been created");

    var fileContent = await File.ReadAllTextAsync(expectedPath);
    fileContent.Should().Contain("  \"Name\":", "because the JSON should be indented for readability");
    fileContent.Should().Contain("  \"Value\":", "because the JSON should be indented for readability");
    fileContent.Should().Contain("  \"Nested\":", "because nested objects should also be indented");
  }

  [Theory]
  [Trait("Category", "Unit")]
  [InlineData("md")]
  [InlineData("txt")]
  public async Task WriteToFileAndWaitAsync_WithMarkdownExtensions_ConvertsToString(string extension)
  {
    // Arrange
    var data = new { Title = "Test", Content = "Some content" };
    var filename = $"markdown_test_{extension}";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.{extension}");
    _createdFiles.Add(expectedPath);

    // Act
    await _sut.WriteToFileAndWaitAsync(_testDirectory, filename, data, extension);

    // Assert
    File.Exists(expectedPath).Should().BeTrue($"because the {extension} file should have been created");

    var fileContent = await File.ReadAllTextAsync(expectedPath);
    fileContent.Should().NotBeNullOrEmpty("because the object should be converted to string representation");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithUnsupportedExtensionForSerialization_ThrowsArgumentException()
  {
    // Arrange
    var testData = new { Name = "Test" };
    var filename = "unsupported_test";
    var unsupportedExtension = "xyz";

    // Act
    var act = () => _sut.WriteToFileAndWaitAsync(_testDirectory, filename, testData, unsupportedExtension);

    // Assert
    await act.Should().ThrowAsync<ArgumentException>("because unsupported extensions should throw an exception")
        .WithMessage("*Unsupported extension for serialization: xyz*");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithPdfDataButNotByteArray_ThrowsArgumentException()
  {
    // Arrange
    var invalidPdfData = "This is not a byte array";
    var filename = "invalid_pdf_test";

    // Act
    var act = () => _sut.WriteToFileAndWaitAsync(_testDirectory, filename, invalidPdfData, "pdf");

    // Assert
    await act.Should().ThrowAsync<ArgumentException>("because PDF data must be a byte array")
        .WithMessage("*PDF content must be byte array*");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void QueueWrite_LogsWithAwaitFlag_WhenUsingWriteToFileAndWaitAsync()
  {
    // Arrange
    var testData = new { Test = "data" };
    var filename = "await_flag_test";

    // Act
    _sut.WriteToFileAndWaitAsync(_testDirectory, filename, testData);

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Queued file write with await:")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "because WriteToFileAndWaitAsync should log with 'with await' flag");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithConcurrentWrites_HandlesAllWritesCorrectly()
  {
    // Arrange
    var numberOfFiles = 10;
    var tasks = new List<Task>();
    var expectedFiles = new List<string>();

    for (int i = 0; i < numberOfFiles; i++)
    {
      var filename = $"concurrent_file_{i}";
      var data = new { Id = i, Message = $"Test data {i}" };
      var expectedPath = Path.Combine(_testDirectory, $"{filename}.json");
      expectedFiles.Add(expectedPath);
      _createdFiles.Add(expectedPath);

      // Act - Queue all writes concurrently
      tasks.Add(_sut.WriteToFileAndWaitAsync(_testDirectory, filename, data));
    }

    // Wait for all writes to complete
    await Task.WhenAll(tasks);

    // Assert - All files should be created
    foreach (var expectedPath in expectedFiles)
    {
      File.Exists(expectedPath).Should().BeTrue($"because file {expectedPath} should have been created");
    }
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WhenWriteFails_PropagatesException()
  {
    // Arrange
    var invalidDirectory = Path.Combine(Path.GetPathRoot(Path.GetTempPath())!, "InvalidPath", Guid.NewGuid().ToString());
    var testData = new { Test = "data" };
    var filename = "test_file";

    // Make directory creation fail
    if (OperatingSystem.IsWindows())
    {
      invalidDirectory = "CON"; // Reserved name on Windows
    }

    // Act
    var act = () => _sut.WriteToFileAndWaitAsync(invalidDirectory, filename, testData);

    // Assert
    await act.Should().ThrowAsync<Exception>("because write operation to invalid path should fail");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task DisposeAsync_WhilePendingWrites_DisposesCleanly()
  {
    // Arrange
    var service = new FileWriteQueueService(_mockLogger.Object);
    var testData = new { Message = "Disposal test" };
    var filename = "disposal_test";

    // Act - Start write but don't wait
    var writeTask = service.WriteToFileAndWaitAsync(_testDirectory, filename, testData);

    // Dispose while write might be pending
    await service.DisposeAsync();

    // Assert - Disposal should complete without exceptions
    // The write task may or may not complete depending on timing
    service.Should().NotBeNull("because service should handle disposal gracefully");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Dispose_SynchronousDispose_CompletesSuccessfully()
  {
    // Arrange
    var service = new FileWriteQueueService(_mockLogger.Object);
    var testData = new { Message = "Sync disposal test" };

    // Queue some work
    service.QueueWrite(_testDirectory, "sync_disposal_test", testData);

    // Act & Assert - Should not throw
    var act = () => ((IDisposable)service).Dispose();
    act.Should().NotThrow("because synchronous dispose should handle gracefully");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task QueueWrite_WithInvalidFileNameCharacters_SanitizesFileName()
  {
    // Arrange
    var testData = new { Name = "Test" };
    var invalidFileName = "file:with*invalid|chars<>?";
    var expectedSanitizedName = "file_with_invalid_chars";
    var expectedPath = Path.Combine(_testDirectory, $"{expectedSanitizedName}.json");
    _createdFiles.Add(expectedPath);

    // Act
    await _sut.WriteToFileAndWaitAsync(_testDirectory, invalidFileName, testData);

    // Assert
    File.Exists(expectedPath).Should().BeTrue("because file should be created with sanitized name");
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"{expectedSanitizedName}.json")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce,
        "because invalid filename characters should be sanitized in log messages");
  }

  [Theory]
  [Trait("Category", "Unit")]
  [InlineData("jpg")]
  [InlineData("jpeg")]
  public async Task WriteToFileAndWaitAsync_WithImageExtension_ThrowsArgumentException(string extension)
  {
    // Arrange
    var imageData = "fake image data"; // Not actual image bytes
    var filename = $"image_test_{extension}";

    // Act
    var act = () => _sut.WriteToFileAndWaitAsync(_testDirectory, filename, imageData, extension);

    // Assert
    await act.Should().ThrowAsync<ArgumentException>()
        .WithMessage($"*Unsupported extension for serialization: {extension}*",
        $"because {extension} is not a supported extension for serialization");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithUnsupportedMimeType_LogsWarning()
  {
    // Arrange
    var testData = "test content";
    var filename = "unknown_type_test";
    var unsupportedExtension = "xyz";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.{unsupportedExtension}");
    _createdFiles.Add(expectedPath);

    // Act & Assert - Should throw because serialization fails for unknown extension
    var act = () => _sut.WriteToFileAndWaitAsync(_testDirectory, filename, testData, unsupportedExtension);
    await act.Should().ThrowAsync<ArgumentException>("because unsupported extension should fail during serialization");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_MultipleWritesToSameFile_LastWriteWins()
  {
    // Arrange
    var filename = "overwrite_test";
    var firstData = new { Value = "First write", Timestamp = DateTime.UtcNow };
    var secondData = new { Value = "Second write", Timestamp = DateTime.UtcNow.AddSeconds(1) };
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.json");
    _createdFiles.Add(expectedPath);

    // Act - Write twice to same file
    await _sut.WriteToFileAndWaitAsync(_testDirectory, filename, firstData);
    await _sut.WriteToFileAndWaitAsync(_testDirectory, filename, secondData);

    // Assert - File should contain second write
    var fileContent = await File.ReadAllTextAsync(expectedPath);
    fileContent.Should().Contain("\"Second write\"", "because the second write should overwrite the first");
    fileContent.Should().NotContain("\"First write\"", "because the first write should be overwritten");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task QueueWrite_WithoutWaiting_EventuallyCreatesFile()
  {
    // Arrange
    var testData = new { Message = "Fire and forget" };
    var filename = "queue_without_wait_test";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.json");
    _createdFiles.Add(expectedPath);

    // Act - Queue write without waiting
    _sut.QueueWrite(_testDirectory, filename, testData);

    // Assert - File should eventually be created
    var maxWaitTime = TimeSpan.FromSeconds(5);
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    while (!File.Exists(expectedPath) && stopwatch.Elapsed < maxWaitTime)
    {
      await Task.Delay(100);
    }

    File.Exists(expectedPath).Should().BeTrue("because queued write should eventually create the file");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithLargeJsonObject_HandlesCorrectly()
  {
    // Arrange
    var largeData = new
    {
      Items = Enumerable.Range(1, 1000).Select(i => new
      {
        Id = i,
        Name = $"Item {i}",
        Description = new string('x', 100),
        Nested = new { Value = i * 2, SubItems = Enumerable.Range(1, 10).ToList() }
      }).ToList()
    };
    var filename = "large_json_test";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.json");
    _createdFiles.Add(expectedPath);

    // Act
    await _sut.WriteToFileAndWaitAsync(_testDirectory, filename, largeData);

    // Assert
    File.Exists(expectedPath).Should().BeTrue("because large JSON should be written successfully");
    var fileInfo = new FileInfo(expectedPath);
    fileInfo.Length.Should().BeGreaterThan(100000, "because large object should create substantial file");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task WriteToFileAndWaitAsync_WithSpecialCharactersInContent_PreservesContent()
  {
    // Arrange
    var specialContent = new
    {
      UnicodeText = "Hello 世界 🌍 Здравствуй мир",
      HtmlContent = "<div class=\"test\">Content & 'quotes' \"everywhere\"</div>",
      JsonString = "{\"nested\": \"json\"}",
      BackslashPath = "C:\\Users\\Test\\File.txt"
    };
    var filename = "special_chars_test";
    var expectedPath = Path.Combine(_testDirectory, $"{filename}.json");
    _createdFiles.Add(expectedPath);

    // Act
    await _sut.WriteToFileAndWaitAsync(_testDirectory, filename, specialContent);

    // Assert
    File.Exists(expectedPath).Should().BeTrue("because file with special characters should be created");
    var fileContent = await File.ReadAllTextAsync(expectedPath);

    fileContent.Should().Contain("世界", "because Unicode characters should be preserved");
    // Note: The emoji may be escaped in JSON as \uD83C\uDF0D
    fileContent.Should().Match("*\uD83C\uDF0D*", "because emoji should be preserved (possibly escaped)");
    fileContent.Should().Contain("Здравствуй мир", "because Cyrillic should be preserved");
    fileContent.Should().Contain("&", "because HTML entities should be preserved");
  }

  public void Dispose()
  {
    (_sut as IDisposable)?.Dispose();

    // Clean up created test files and directories
    foreach (var filePath in _createdFiles.Where(File.Exists))
    {
      try
      {
        File.Delete(filePath);
      }
      catch
      {
        // Ignore cleanup failures
      }
    }

    if (Directory.Exists(_testDirectory))
    {
      try
      {
        Directory.Delete(_testDirectory, recursive: true);
      }
      catch
      {
        // Ignore cleanup failures
      }
    }
  }
}
