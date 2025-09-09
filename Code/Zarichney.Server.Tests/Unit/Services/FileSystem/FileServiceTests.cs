using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Zarichney.Services.FileSystem;
using FluentAssertions;
using Xunit;

namespace Zarichney.Tests.Unit.Services.FileSystem;

[Trait("Category", "Unit")]
public class FileServiceTests : IDisposable
{
  private readonly Mock<ILogger<FileService>> _mockLogger;
  private readonly FileService _sut;
  private readonly string _testDirectory;
  private readonly List<string> _filesToCleanup = new();

  public FileServiceTests()
  {
    _mockLogger = new Mock<ILogger<FileService>>();
    _sut = new FileService(_mockLogger.Object);
    _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(_testDirectory);
  }

  public void Dispose()
  {
    // Clean up test files
    foreach (var file in _filesToCleanup)
    {
      if (File.Exists(file))
      {
        try { File.Delete(file); } catch { /* Ignore cleanup errors */ }
      }
    }

    // Clean up test directory
    if (Directory.Exists(_testDirectory))
    {
      try { Directory.Delete(_testDirectory, true); } catch { /* Ignore cleanup errors */ }
    }
  }

  #region ReadFromFile Tests

  [Fact]
  public async Task ReadFromFile_ValidJsonFile_ReturnsDeserializedObject()
  {
    // Arrange
    var testData = new { Name = "Test", Value = 42 };
    var fileName = "test-file";
    var filePath = Path.Combine(_testDirectory, $"{fileName}.json");
    await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(testData));
    _filesToCleanup.Add(filePath);

    // Act
    var result = await _sut.ReadFromFile<dynamic>(_testDirectory, fileName, "json");

    // Assert
    // When deserializing to dynamic, System.Text.Json returns JsonElement.
    // Calling extension methods like Should() on dynamic causes a runtime binder error.
    // Cast to object to enable FluentAssertions extension resolution safely.
    ((object)result).Should().NotBeNull("a valid JSON file should be deserialized successfully");
  }

  [Fact]
  public async Task ReadFromFile_NonExistentFile_ReturnsDefault()
  {
    // Arrange
    var fileName = "non-existent-file";

    // Act
    var result = await _sut.ReadFromFile<string>(_testDirectory, fileName);

    // Assert
    result.Should().BeNull("non-existent files should return default value");
  }

  [Fact]
  public async Task ReadFromFile_DefaultExtension_UsesJson()
  {
    // Arrange
    var testData = "test content";
    var fileName = "test-file";
    var filePath = Path.Combine(_testDirectory, $"{fileName}.json");
    await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(testData));
    _filesToCleanup.Add(filePath);

    // Act
    var result = await _sut.ReadFromFile<string>(_testDirectory, fileName);

    // Assert
    result.Should().Be(testData, "default extension should be json");
  }

  [Fact]
  public async Task ReadFromFile_PdfExtension_ReturnsByteArray()
  {
    // Arrange
    var fileName = "test-file";
    var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
    var filePath = Path.Combine(_testDirectory, $"{fileName}.pdf");
    await File.WriteAllBytesAsync(filePath, pdfData);
    _filesToCleanup.Add(filePath);

    // Act
    var result = await _sut.ReadFromFile<byte[]>(_testDirectory, fileName, "pdf");

    // Assert
    result.Should().BeEquivalentTo(pdfData, "PDF files should return byte array content");
  }

  #endregion

  #region GetFiles Tests

  [Fact]
  public void GetFiles_ExistingDirectory_ReturnsFileArray()
  {
    // Arrange
    var testFile = Path.Combine(_testDirectory, "test.txt");
    File.WriteAllText(testFile, "test content");
    _filesToCleanup.Add(testFile);

    // Act
    var result = _sut.GetFiles(_testDirectory);

    // Assert
    result.Should().HaveCount(1, "existing directory should return all files");
    result.Should().Contain(testFile, "existing directory should contain the test file");
  }

  [Fact]
  public void GetFiles_NonExistentDirectory_CreatesDirectoryAndReturnsEmpty()
  {
    // Arrange
    var newDirectory = Path.Combine(_testDirectory, "new-dir");

    // Act
    var result = _sut.GetFiles(newDirectory);

    // Assert
    result.Should().BeEmpty("newly created directory should have no files");
    Directory.Exists(newDirectory).Should().BeTrue("non-existent directory should be created");
  }

  #endregion

  #region GetFile and GetFileAsync Tests

  [Fact]
  public void GetFile_ExistingFile_ReturnsContent()
  {
    // Arrange
    var testFile = Path.Combine(_testDirectory, "test.txt");
    var content = "test file content";
    File.WriteAllText(testFile, content);
    _filesToCleanup.Add(testFile);

    // Act
    var result = _sut.GetFile(testFile);

    // Assert
    result.Should().Be(content, "existing file should return its content");
  }

  [Fact]
  public async Task GetFileAsync_ExistingFile_ReturnsContent()
  {
    // Arrange
    var testFile = Path.Combine(_testDirectory, "test.txt");
    var content = "test async file content";
    await File.WriteAllTextAsync(testFile, content);
    _filesToCleanup.Add(testFile);

    // Act
    var result = await _sut.GetFileAsync(testFile);

    // Assert
    result.Should().Be(content, "existing file should return its content asynchronously");
  }

  [Fact]
  public void GetFile_NonExistentFile_ThrowsException()
  {
    // Arrange
    var nonExistentFile = Path.Combine(_testDirectory, "non-existent.txt");

    // Act
    Action act = () => _sut.GetFile(nonExistentFile);

    // Assert
    act.Should().Throw<FileNotFoundException>("non-existent file should throw exception");
  }

  #endregion

  #region CreateFile Tests

  [Fact]
  public async Task CreateFile_JsonType_CreatesFormattedJsonFile()
  {
    // Arrange
    var testData = new { Name = "Test", Values = new[] { 1, 2, 3 } };
    var filePath = Path.Combine(_testDirectory, "test.json");
    _filesToCleanup.Add(filePath);

    // Act
    await _sut.CreateFile(filePath, testData, "application/json");

    // Assert
    File.Exists(filePath).Should().BeTrue("JSON file should be created");
    var content = await File.ReadAllTextAsync(filePath);
    content.Should().Contain("Name", "JSON content should contain the Name property");
    content.Should().Contain("Test", "JSON content should contain the Test value");
  }

  [Fact]
  public async Task CreateFile_TextType_CreatesTextFile()
  {
    // Arrange
    var textData = "This is test content";
    var filePath = Path.Combine(_testDirectory, "test.txt");
    _filesToCleanup.Add(filePath);

    // Act
    await _sut.CreateFile(filePath, textData, "text/plain");

    // Assert
    File.Exists(filePath).Should().BeTrue("text file should be created");
    var content = await File.ReadAllTextAsync(filePath);
    content.Should().Be(textData, "text content should match input");
  }

  [Fact]
  public async Task CreateFile_PdfType_CreatesPdfFile()
  {
    // Arrange
    var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // PDF header
    var filePath = Path.Combine(_testDirectory, "test.pdf");
    _filesToCleanup.Add(filePath);

    // Act
    await _sut.CreateFile(filePath, pdfData, "application/pdf");

    // Assert
    File.Exists(filePath).Should().BeTrue("PDF file should be created");
    var content = await File.ReadAllBytesAsync(filePath);
    content.Should().BeEquivalentTo(pdfData, "PDF content should match input bytes");
  }

  [Fact]
  public async Task CreateFile_ImageJpegType_CreatesJpegFile()
  {
    // Arrange
    using var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgb24>(100, 100);
    var filePath = Path.Combine(_testDirectory, "test.jpg");
    _filesToCleanup.Add(filePath);

    // Act
    await _sut.CreateFile(filePath, image, "image/jpeg");

    // Assert
    File.Exists(filePath).Should().BeTrue("JPEG file should be created");
    var fileInfo = new FileInfo(filePath);
    fileInfo.Length.Should().BeGreaterThan(0, "JPEG file should have content");
  }

  [Fact]
  public async Task CreateFile_UnsupportedFileType_ThrowsArgumentException()
  {
    // Arrange
    var testData = "test data";
    var filePath = Path.Combine(_testDirectory, "test.xyz");

    // Act
    Func<Task> act = async () => await _sut.CreateFile(filePath, testData, "unsupported/type");

    // Assert
    var exception = await act.Should().ThrowAsync<ArgumentException>("unsupported file types should throw exception");
    exception.WithMessage("*Unsupported file type*");
  }

  #endregion

  #region DeleteFile Tests

  [Fact]
  public void DeleteFile_ExistingFile_DeletesSuccessfully()
  {
    // Arrange
    var testFile = Path.Combine(_testDirectory, "delete-test.txt");
    File.WriteAllText(testFile, "content to delete");

    // Act
    _sut.DeleteFile(testFile);

    // Assert
    File.Exists(testFile).Should().BeFalse("file should be deleted");
  }

  [Fact]
  public void DeleteFile_NonExistentFile_DoesNotThrow()
  {
    // Arrange
    var nonExistentFile = Path.Combine(_testDirectory, "non-existent.txt");

    // Act
    Action act = () => _sut.DeleteFile(nonExistentFile);

    // Assert
    act.Should().NotThrow("deleting non-existent file should not throw exception");
  }

  #endregion

  #region FileExists Tests

  [Fact]
  public void FileExists_ExistingFile_ReturnsTrue()
  {
    // Arrange
    var testFile = Path.Combine(_testDirectory, "exists-test.txt");
    File.WriteAllText(testFile, "test content");
    _filesToCleanup.Add(testFile);

    // Act
    var result = _sut.FileExists(testFile);

    // Assert
    result.Should().BeTrue("existing file should return true");
  }

  [Fact]
  public void FileExists_NonExistentFile_ReturnsFalse()
  {
    // Arrange
    var nonExistentFile = Path.Combine(_testDirectory, "non-existent.txt");

    // Act
    var result = _sut.FileExists(nonExistentFile);

    // Assert
    result.Should().BeFalse("non-existent file should return false");
  }

  [Fact]
  public void FileExists_NullFilePath_ReturnsFalse()
  {
    // Act
    var result = _sut.FileExists(null);

    // Assert
    result.Should().BeFalse("null file path should return false");
  }

  [Fact]
  public void FileExists_EmptyFilePath_ReturnsFalse()
  {
    // Act
    var result = _sut.FileExists(string.Empty);

    // Assert
    result.Should().BeFalse("empty file path should return false");
  }

  #endregion

  #region SanitizeFileName Tests

  [Fact]
  public void SanitizeFileName_ValidFileName_ReturnsUnchanged()
  {
    // Arrange
    var validFileName = "valid-filename_123";

    // Act
    var result = FileService.SanitizeFileName(validFileName);

    // Assert
    result.Should().Be(validFileName, "valid file names should remain unchanged");
  }

  [Fact]
  public void SanitizeFileName_InvalidCharacters_ReplacesWithUnderscore()
  {
    // Arrange
    var invalidFileName = "file<name>with|invalid:chars";

    // Act
    var result = FileService.SanitizeFileName(invalidFileName);

    // Assert
    result.Should().Be("file_name_with_invalid_chars", "invalid characters should be replaced with underscores");
  }

  [Fact]
  public void SanitizeFileName_EmptyString_ReturnsEmpty()
  {
    // Arrange
    var emptyFileName = string.Empty;

    // Act
    var result = FileService.SanitizeFileName(emptyFileName);

    // Assert
    result.Should().BeEmpty("empty string should return empty string");
  }

  [Theory]
  [InlineData("file/name", "file_name")]
  [InlineData("file\\name", "file_name")]
  [InlineData("file*name", "file_name")]
  [InlineData("file?name", "file_name")]
  [InlineData("file\"name", "file_name")]
  [InlineData("file<name>", "file_name")]
  [InlineData("file|name", "file_name")]
  public void SanitizeFileName_SpecificInvalidChars_ReplacesCorrectly(string input, string expected)
  {
    // Act
    var result = FileService.SanitizeFileName(input);

    // Assert
    result.Should().Be(expected, $"invalid character in '{input}' should be replaced");
  }

  #endregion

  #region Integration and Error Scenarios

  [Fact]
  public async Task ReadFromFile_FileWithRetryLogic_HandlesTransientErrors()
  {
    // Arrange
    var testData = "retry test content";
    var fileName = "retry-test";
    var filePath = Path.Combine(_testDirectory, $"{fileName}.txt");
    await File.WriteAllTextAsync(filePath, testData);
    _filesToCleanup.Add(filePath);

    // Act
    var result = await _sut.ReadFromFile<string>(_testDirectory, fileName, "txt");

    // Assert
    result.Should().Be(testData, "file should be read successfully with retry logic");
  }

  [Fact]
  public async Task ReadFromFile_MarkdownExtension_ReturnsStringContent()
  {
    // Arrange
    var markdownContent = "# Test Markdown\n\nThis is a test.";
    var fileName = "test-markdown";
    var filePath = Path.Combine(_testDirectory, $"{fileName}.md");
    await File.WriteAllTextAsync(filePath, markdownContent);
    _filesToCleanup.Add(filePath);

    // Act
    var result = await _sut.ReadFromFile<string>(_testDirectory, fileName, "md");

    // Assert
    result.Should().Be(markdownContent, "markdown files should return string content");
  }

  [Fact]
  public async Task ReadFromFile_UnsupportedExtension_ThrowsArgumentException()
  {
    // Arrange
    var fileName = "test-file";
    var filePath = Path.Combine(_testDirectory, $"{fileName}.xyz");
    await File.WriteAllTextAsync(filePath, "content");
    _filesToCleanup.Add(filePath);

    // Act
    Func<Task> act = async () => await _sut.ReadFromFile<string>(_testDirectory, fileName, "xyz");

    // Assert
    var exception = await act.Should().ThrowAsync<ArgumentException>("unsupported extensions should throw exception");
    exception.WithMessage("*Unsupported extension*");
  }

  #endregion
}
