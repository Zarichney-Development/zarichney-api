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