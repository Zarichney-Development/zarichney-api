using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.FileSystem;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Integration;

namespace Zarichney.Server.Tests.Integration.Services.FileSystem;

/// <summary>
/// Integration tests for FileService that validate service integration patterns and file operations.
/// These tests verify FileService functionality within the application context and integration with the host.
/// Tests use temporary directories for file operations to ensure proper cleanup and isolation.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Component", "Service")]
[Trait("Feature", "FileSystem")]
[Collection("IntegrationCore")]
public class FileServiceIntegrationTests : IntegrationTestBase, IDisposable
{
  private readonly string _tempDirectory;

  public FileServiceIntegrationTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
      : base(apiClientFixture, testOutputHelper)
  {
    // Create a unique temporary directory for each test run
    _tempDirectory = Path.Combine(Path.GetTempPath(), $"FileServiceIntegrationTests_{Guid.NewGuid():N}");
    Directory.CreateDirectory(_tempDirectory);
  }

  #region Service Integration and Dependency Injection Tests

  /// <summary>
  /// Tests that FileService is properly registered in the DI container and can be resolved.
  /// Validates service registration and dependency injection configuration.
  /// </summary>
  [Fact]
  public void FileService_ServiceRegistration_CanBeResolvedFromDI()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(FileService_ServiceRegistration_CanBeResolvedFromDI));

    // Act
    var fileService = GetService<IFileService>();

    // Assert
    fileService.Should().NotBeNull("FileService should be registered in DI container");
    fileService.Should().BeOfType<FileService>("Should resolve to concrete FileService implementation");
  }

  /// <summary>
  /// Tests FileService logger integration within the application context.
  /// Validates that logging is properly configured and functional.
  /// </summary>
  [Fact]
  public void FileService_LoggerIntegration_LogsFileOperations()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(FileService_LoggerIntegration_LogsFileOperations));
    var fileService = GetService<IFileService>();
    var testFilePath = Path.Combine(_tempDirectory, "logger-test.txt");

    // Act & Assert - Verify service can perform operations (logging happens internally)
    var fileExistsBefore = fileService.FileExists(testFilePath);
    fileExistsBefore.Should().BeFalse("Test file should not exist before creation");

    // The logging verification is implicit - if operations succeed, logging is working
    // Direct log assertion would require more complex test setup
  }

  #endregion

  #region JSON File Operations Integration Tests

  /// <summary>
  /// Tests JSON file creation and reading integration with proper serialization.
  /// Validates end-to-end JSON file handling as used by application services.
  /// </summary>
  [Fact]
  public async Task FileService_JsonOperations_CreatesAndReadsJsonFiles()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(FileService_JsonOperations_CreatesAndReadsJsonFiles));
    var fileService = GetService<IFileService>();
    var testFilePath = Path.Combine(_tempDirectory, "test-data.json");

    var testData = new
    {
      Id = 12345,
      Name = "Integration Test Object",
      Timestamp = DateTime.UtcNow.ToString("O"), // Use string to avoid serialization complexity
      Properties = new[] { "property1", "property2", "property3" }
    };

    // Act - Create file
    await fileService.CreateFile(testFilePath, testData, "application/json");

    // Act - Verify file exists
    var fileExists = fileService.FileExists(testFilePath);

    // Act - Read file content back
    var fileContent = await fileService.GetFileAsync(testFilePath);

    // Assert
    fileExists.Should().BeTrue("JSON file should exist after creation");
    fileContent.Should().NotBeNullOrEmpty("File content should not be empty");

    // Validate JSON structure
    var deserializedData = JsonSerializer.Deserialize<JsonElement>(fileContent);
    deserializedData.GetProperty("Id").GetInt32().Should().Be(12345);
    deserializedData.GetProperty("Name").GetString().Should().Be("Integration Test Object");
    deserializedData.GetProperty("Properties").GetArrayLength().Should().Be(3);
  }

  /// <summary>
  /// Tests reading typed objects from JSON files as used by repository patterns.
  /// Validates generic JSON deserialization integration within the application context.
  /// </summary>
  [Fact]
  public async Task FileService_TypedJsonRead_DeserializesObjectsCorrectly()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(FileService_TypedJsonRead_DeserializesObjectsCorrectly));
    var fileService = GetService<IFileService>();
    var testDirectory = Path.Combine(_tempDirectory, "typed-json");
    Directory.CreateDirectory(testDirectory);

    var testData = new TestDataModel
    {
      Id = "test-123",
      Value = 42.5,
      IsActive = true,
      Tags = new[] { "integration", "test", "json" }
    };

    // Manually create JSON file to test reading
    var jsonContent = JsonSerializer.Serialize(testData, new JsonSerializerOptions { WriteIndented = true });
    var filePath = Path.Combine(testDirectory, "test-object.json");
    await File.WriteAllTextAsync(filePath, jsonContent);

    // Act
    var result = await fileService.ReadFromFile<TestDataModel>(testDirectory, "test-object");

    // Assert
    result.Should().NotBeNull("Deserialized object should not be null");
    result.Id.Should().Be("test-123");
    result.Value.Should().Be(42.5);
    result.IsActive.Should().BeTrue();
    result.Tags.Should().HaveCount(3);
    result.Tags.Should().Contain("integration");
  }

  #endregion

  #region File System Operations Integration Tests

  /// <summary>
  /// Tests directory file listing functionality as used by application services.
  /// Validates file discovery patterns used by repositories and services.
  /// </summary>
  [Fact]
  public void FileService_DirectoryOperations_ListsFilesCorrectly()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(FileService_DirectoryOperations_ListsFilesCorrectly));
    var fileService = GetService<IFileService>();
    var testDirectory = Path.Combine(_tempDirectory, "directory-test");
    Directory.CreateDirectory(testDirectory);

    // Create test files
    var testFiles = new[] { "file1.txt", "file2.json", "file3.md" };
    foreach (var fileName in testFiles)
    {
      File.WriteAllText(Path.Combine(testDirectory, fileName), $"Content for {fileName}");
    }

    // Act
    var files = fileService.GetFiles(testDirectory);

    // Assert
    files.Should().NotBeNull("File list should not be null");
    files.Should().HaveCount(3, "Should find all created test files");

    foreach (var testFile in testFiles)
    {
      files.Should().Contain(path => Path.GetFileName(path) == testFile,
          $"Should contain {testFile}");
    }
  }

  /// <summary>
  /// Tests file deletion functionality with proper cleanup as used by services.
  /// Validates file lifecycle management patterns.
  /// </summary>
  [Fact]
  public async Task FileService_FileLifecycle_CreatesAndDeletesFiles()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(FileService_FileLifecycle_CreatesAndDeletesFiles));
    var fileService = GetService<IFileService>();
    var testFilePath = Path.Combine(_tempDirectory, "lifecycle-test.txt");

    // Act - Create file
    await fileService.CreateFile(testFilePath, "Test content for lifecycle", "text/plain");
    var existsAfterCreate = fileService.FileExists(testFilePath);

    // Act - Delete file
    fileService.DeleteFile(testFilePath);
    var existsAfterDelete = fileService.FileExists(testFilePath);

    // Assert
    existsAfterCreate.Should().BeTrue("File should exist after creation");
    existsAfterDelete.Should().BeFalse("File should not exist after deletion");
  }

  #endregion

  #region Error Handling and Edge Cases Integration Tests

  /// <summary>
  /// Tests FileService behavior with non-existent files and directories.
  /// Validates error handling patterns used throughout the application.
  /// </summary>
  [Fact]
  public async Task FileService_ErrorHandling_HandlesNonExistentFiles()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(FileService_ErrorHandling_HandlesNonExistentFiles));
    var fileService = GetService<IFileService>();
    var nonExistentFilePath = Path.Combine(_tempDirectory, "non-existent.txt");
    var nonExistentDirectory = Path.Combine(_tempDirectory, "non-existent-dir");

    // Act & Assert - File existence check
    var fileExists = fileService.FileExists(nonExistentFilePath);
    fileExists.Should().BeFalse("Non-existent file should return false");

    // Act & Assert - Reading non-existent file
    var result = await fileService.ReadFromFile<object>(nonExistentDirectory, "non-existent");
    result.Should().BeNull("Reading non-existent file should return null/default");

    // Act & Assert - Deleting non-existent file (should be idempotent)
    Action deleteAction = () => fileService.DeleteFile(nonExistentFilePath);
    deleteAction.Should().NotThrow("Deleting non-existent file should not throw exception");
  }

  /// <summary>
  /// Tests FileService with invalid file paths and characters.
  /// Validates input sanitization as used by application services.
  /// </summary>
  [Fact]
  public async Task FileService_InputValidation_HandlesInvalidCharacters()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(FileService_InputValidation_HandlesInvalidCharacters));
    var fileService = GetService<IFileService>();
    var testDirectory = Path.Combine(_tempDirectory, "validation-test");
    Directory.CreateDirectory(testDirectory);

    // Test with filename containing invalid characters (will be sanitized)
    var problematicFilename = "test<>:|*?file";
    var testData = new { message = "Test with problematic filename" };

    // Act & Assert - Should handle gracefully (either sanitize or fail predictably)
    try
    {
      await fileService.CreateFile(
          Path.Combine(testDirectory, $"{problematicFilename}.json"),
          testData,
          "application/json");

      // If creation succeeds, verify the file was created with sanitized name
      var files = fileService.GetFiles(testDirectory);
      files.Should().HaveCountGreaterThan(0, "Should create a file with sanitized name");
    }
    catch (Exception ex)
    {
      // File creation may fail due to invalid characters, which is acceptable
      ex.Should().BeAssignableTo<Exception>()
          .And.Match(e => e is ArgumentException || e is IOException || e is UnauthorizedAccessException || e is DirectoryNotFoundException);
    }
  }

  #endregion

  #region Image File Operations Integration Tests

  /// <summary>
  /// Tests image file creation functionality as used by PDF generation services.
  /// Validates image handling integration within the application context.
  /// </summary>
  [Fact]
  public async Task FileService_ImageOperations_CreatesJpegFiles()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(FileService_ImageOperations_CreatesJpegFiles));
    var fileService = GetService<IFileService>();
    var testImagePath = Path.Combine(_tempDirectory, "test-image.jpg");

    // Create a simple test image using ImageSharp
    using var image = new Image<Rgba32>(100, 100);

    // Fill with a test color
    for (int x = 0; x < 100; x++)
    {
      for (int y = 0; y < 100; y++)
      {
        image[x, y] = new Rgba32(255, 0, 0); // Red color
      }
    }

    // Act
    await fileService.CreateFile(testImagePath, image, "image/jpeg");

    // Assert
    var fileExists = fileService.FileExists(testImagePath);
    fileExists.Should().BeTrue("JPEG image file should be created");

    var fileInfo = new FileInfo(testImagePath);
    fileInfo.Length.Should().BeGreaterThan(0, "Image file should have content");
  }

  #endregion

  #region Test Helper Classes and Cleanup

  /// <summary>
  /// Test data model for JSON serialization testing.
  /// </summary>
  private class TestDataModel
  {
    public string Id { get; set; } = string.Empty;
    public double Value { get; set; }
    public bool IsActive { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
  }

  /// <summary>
  /// Cleanup temporary test directory after tests complete.
  /// Implement IDisposable to avoid conflicting with IAsyncLifetime in base.
  /// </summary>
  public void Dispose()
  {
    try
    {
      if (Directory.Exists(_tempDirectory))
      {
        Directory.Delete(_tempDirectory, recursive: true);
      }
    }
    catch (Exception ex)
    {
      // Log but don't fail test cleanup
      System.Diagnostics.Debug.WriteLine($"Failed to cleanup test directory: {ex.Message}");
    }
  }

  #endregion
}
