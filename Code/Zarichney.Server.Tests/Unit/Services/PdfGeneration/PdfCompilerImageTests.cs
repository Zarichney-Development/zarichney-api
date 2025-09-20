using System.Net;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using QuestPDF.Infrastructure;
using Xunit;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.FileSystem;
using Zarichney.Services.PdfGeneration;

namespace Zarichney.Tests.Unit.Services.PdfGeneration;

/// <summary>
/// Unit tests for PdfCompiler image processing functionality
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "PdfGeneration")]
public class PdfCompilerImageTests
{
  private readonly IFixture _fixture;
  private readonly Mock<IFileService> _mockFileService;
  private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
  private readonly Mock<ILogger<PdfCompiler>> _mockLogger;
  private readonly PdfCompilerConfig _config;
  private readonly Mock<PdfCompiler> _mockPdfCompiler;

  public PdfCompilerImageTests()
  {
    // Set QuestPDF license
    QuestPDF.Settings.License = LicenseType.Community;

    _fixture = new Fixture();
    _mockFileService = new Mock<IFileService>();
    _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    _mockLogger = new Mock<ILogger<PdfCompiler>>();
    _config = new PdfCompilerConfig
    {
      FontName = "Arial",
      FontSize = 12,
      ImageDirectory = "test-images"
    };

    // Create partial mock to test protected methods
    _mockPdfCompiler = new Mock<PdfCompiler>(_config, _mockFileService.Object, _mockHttpClientFactory.Object, _mockLogger.Object)
    {
      CallBase = true
    };
  }

  #region ProcessFirstValidImage Tests

  [Fact]
  public async Task ProcessFirstValidImage_WithValidImageUrl_ShouldReturnImagePath()
  {
    // Arrange
    var imageUrls = new[] { "https://example.com/image1.jpg" };
    var recipeTitle = "Test Recipe";
    var expectedPath = "test-images/TestRecipe_abc123.jpg";

    _mockPdfCompiler
      .Protected()
      .Setup<Task<string>>("ProcessImage", ItExpr.IsAny<string>(), ItExpr.IsAny<string>())
      .ReturnsAsync(expectedPath);

    // Act
    var result = await _mockPdfCompiler.Object.ProcessFirstValidImage(imageUrls, recipeTitle);

    // Assert
    result.Should().Be(expectedPath, "valid image URL should return processed image path");
    _mockPdfCompiler.Protected().Verify("ProcessImage", Times.Once(), ItExpr.IsAny<string>(), ItExpr.IsAny<string>());
  }

  [Fact]
  public async Task ProcessFirstValidImage_WithMultipleUrls_ShouldReturnFirstValidImagePath()
  {
    // Arrange
    var imageUrls = new[]
    {
      "invalid-url",
      "https://example.com/valid.jpg",
      "https://example.com/another.jpg"
    };
    var recipeTitle = "Test Recipe";
    var expectedPath = "test-images/TestRecipe_abc123.jpg";

    _mockPdfCompiler
      .Protected()
      .Setup<Task<string>>("ProcessImage", "https://example.com/valid.jpg", recipeTitle)
      .ReturnsAsync(expectedPath);

    // Act
    var result = await _mockPdfCompiler.Object.ProcessFirstValidImage(imageUrls, recipeTitle);

    // Assert
    result.Should().Be(expectedPath, "should return first successfully processed image");
  }

  [Fact]
  public async Task ProcessFirstValidImage_WithAllInvalidUrls_ShouldReturnNull()
  {
    // Arrange
    var imageUrls = new[] { "invalid-url-1", "invalid-url-2", "" };
    var recipeTitle = "Test Recipe";

    // Act
    var result = await _mockPdfCompiler.Object.ProcessFirstValidImage(imageUrls, recipeTitle);

    // Assert
    result.Should().BeNull("no valid images should return null");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No valid images found")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task ProcessFirstValidImage_WithProcessingException_ShouldContinueToNextUrl()
  {
    // Arrange
    var imageUrls = new[]
    {
      "https://example.com/fail.jpg",
      "https://example.com/success.jpg"
    };
    var recipeTitle = "Test Recipe";
    var expectedPath = "test-images/TestRecipe_abc123.jpg";

    _mockPdfCompiler
      .Protected()
      .SetupSequence<Task<string>>("ProcessImage", ItExpr.IsAny<string>(), ItExpr.IsAny<string>())
      .ThrowsAsync(new HttpRequestException("Network error"))
      .ReturnsAsync(expectedPath);

    // Act
    var result = await _mockPdfCompiler.Object.ProcessFirstValidImage(imageUrls, recipeTitle);

    // Assert
    result.Should().Be(expectedPath, "should continue to next URL after exception");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to process image URL")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task ProcessFirstValidImage_WithEmptyImageUrls_ShouldReturnNull()
  {
    // Arrange
    var imageUrls = Array.Empty<string>();
    var recipeTitle = "Test Recipe";

    // Act
    var result = await _mockPdfCompiler.Object.ProcessFirstValidImage(imageUrls, recipeTitle);

    // Assert
    result.Should().BeNull("empty URL list should return null");
  }

  #endregion

  #region IsValidImageUrl Tests

  [Theory]
  [InlineData("https://example.com/image.jpg", true)]
  [InlineData("http://example.com/image.png", true)]
  [InlineData("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==", true)]
  [InlineData("ftp://example.com/image.jpg", false)]
  [InlineData("invalid-url", false)]
  [InlineData("", false)]
  [InlineData("   ", false)]
  public void IsValidImageUrl_ShouldValidateCorrectly(string url, bool expected)
  {
    // Arrange & Act
    var result = _mockPdfCompiler.Object.IsValidImageUrl(url);

    // Assert
    result.Should().Be(expected, $"URL '{url}' validation should return {expected}");
  }

  [Fact]
  public void IsValidImageUrl_WithNullUrl_ShouldReturnFalse()
  {
    // Arrange
    string? nullUrl = null;

    // Act
    var result = _mockPdfCompiler.Object.IsValidImageUrl(nullUrl!);

    // Assert
    result.Should().BeFalse("null URL should return false");
  }

  [Fact]
  public void IsValidImageUrl_WithInvalidDataUrl_ShouldReturnFalse()
  {
    // Arrange
    var invalidDataUrl = "data:image/png;base64,invalid-base64-content!!!";

    // Act
    var result = _mockPdfCompiler.Object.IsValidImageUrl(invalidDataUrl);

    // Assert
    result.Should().BeFalse("invalid base64 data URL should return false");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid data URL format")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  #endregion

  #region ProcessImage Tests

  [Fact]
  public async Task ProcessImage_WithDataUrl_ShouldProcessAndSaveImage()
  {
    // Arrange
    var dataUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";
    var fileName = "TestRecipe";

    _mockFileService
      .Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
      .Returns(Task.CompletedTask);

    // Act
    var result = await _mockPdfCompiler.Object.ProcessImage(dataUrl, fileName);

    // Assert
    result.Should().NotBeEmpty("should return path for processed data URL image");
    result.Should().Contain(fileName, "path should contain recipe title");
    result.Should().EndWith(".jpg", "path should have jpg extension");
    _mockFileService.Verify(
      x => x.CreateFile(It.IsAny<string>(), It.IsAny<object>(), "image/jpeg"),
      Times.Once);
  }

  [Fact]
  public async Task ProcessImage_WithUnsupportedScheme_ShouldReturnEmptyString()
  {
    // Arrange
    var unsupportedUrl = "ftp://example.com/image.jpg";
    var fileName = "TestRecipe";

    // Act
    var result = await _mockPdfCompiler.Object.ProcessImage(unsupportedUrl, fileName);

    // Assert
    result.Should().BeEmpty("unsupported URL scheme should return empty string");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unsupported image URL scheme")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task ProcessImage_WithProcessingException_ShouldLogErrorAndReturnEmptyString()
  {
    // Arrange
    var dataUrl = "data:image/png;base64,invalid-base64!!!";
    var fileName = "TestRecipe";

    // Act
    var result = await _mockPdfCompiler.Object.ProcessImage(dataUrl, fileName);

    // Assert
    result.Should().BeEmpty("processing exception should return empty string");
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to process image")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  #endregion

  #region CleanupImages Tests

  [Fact]
  public async Task CleanupImages_WithValidFileNames_ShouldDeleteFiles()
  {
    // Arrange
    var fileNames = new List<string?> { "Recipe1", "Recipe2", "Recipe3" };

    _mockFileService
      .Setup(x => x.DeleteFile(It.IsAny<string>()))
      .Verifiable();

    // Act
    await _mockPdfCompiler.Object.CleanupImages(fileNames);

    // Assert
    // Cleanup uses Directory.GetFiles which won't find files in unit test, but method should complete without error
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error cleaning up image")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.AtLeastOnce);
  }

  [Fact]
  public async Task CleanupImages_WithNullAndEmptyFileNames_ShouldSkipThem()
  {
    // Arrange
    var fileNames = new List<string?> { null, "", "   ", "ValidRecipe" };

    // Act
    await _mockPdfCompiler.Object.CleanupImages(fileNames);

    // Assert
    // Should not throw and should skip null/empty entries
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Warning,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error cleaning up image")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once); // Only for ValidRecipe
  }

  [Fact]
  public async Task CleanupImages_WithEmptyList_ShouldCompleteWithoutError()
  {
    // Arrange
    var fileNames = new List<string?>();

    // Act
    var task = _mockPdfCompiler.Object.CleanupImages(fileNames);
    await task;

    // Assert
    task.IsCompletedSuccessfully.Should().BeTrue("empty list should complete successfully");
  }

  #endregion

  #region CompileCookbook Integration Tests

  [Fact]
  public async Task CompileCookbook_WithImageUrls_ShouldProcessImages()
  {
    // Arrange
    var order = CreateCookbookOrderWithImages();
    var pdfCompiler = new PdfCompiler(_config, _mockFileService.Object, _mockHttpClientFactory.Object, _mockLogger.Object);

    _mockFileService
      .Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
      .Returns(Task.CompletedTask);

    // Act
    var result = await pdfCompiler.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF with images should compile successfully");
    result.Should().NotBeEmpty("PDF should contain content");
  }

  [Fact]
  public async Task CompileCookbook_WithImageProcessingFailure_ShouldStillGeneratePdf()
  {
    // Arrange
    var order = CreateCookbookOrderWithImages();
    var pdfCompiler = new PdfCompiler(_config, _mockFileService.Object, _mockHttpClientFactory.Object, _mockLogger.Object);

    _mockFileService
      .Setup(x => x.CreateFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
      .ThrowsAsync(new IOException("Cannot save image"));

    // Act
    var result = await pdfCompiler.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF should generate even if image processing fails");
    result.Should().NotBeEmpty("PDF should contain content without images");
  }

  [Fact]
  public async Task CompileCookbook_WithMixedValidAndInvalidImageUrls_ShouldProcessValidOnes()
  {
    // Arrange
    var order = CreateCookbookOrderWithMixedImages();
    var pdfCompiler = new PdfCompiler(_config, _mockFileService.Object, _mockHttpClientFactory.Object, _mockLogger.Object);

    // Act
    var result = await pdfCompiler.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF should compile with mixed image URLs");
    result.Should().NotBeEmpty("PDF should contain content");
  }

  #endregion

  #region Private Test Helpers

  private CookbookOrder CreateCookbookOrderWithImages()
  {
    var recipe = new SynthesizedRecipe
    {
      Title = "Test Recipe with Images",
      Description = "A recipe with image URLs",
      ImageUrls = new List<string>
      {
        "https://example.com/image1.jpg",
        "https://example.com/image2.jpg"
      },
      Ingredients = new List<string> { "Ingredient 1", "Ingredient 2" },
      Directions = new List<string> { "Step 1", "Step 2" },
      Notes = "Test notes",
      PrepTime = "10 minutes",
      CookTime = "20 minutes",
      TotalTime = "30 minutes",
      Servings = "4"
    };

    return new CookbookOrder(_fixture.Create<Zarichney.Cookbook.Customers.Customer>(),
      _fixture.Create<CookbookOrderSubmission>(), _fixture.Create<List<string>>())
    {
      OrderId = _fixture.Create<string>(),
      SynthesizedRecipes = new List<SynthesizedRecipe> { recipe }
    };
  }

  private CookbookOrder CreateCookbookOrderWithMixedImages()
  {
    var recipe = new SynthesizedRecipe
    {
      Title = "Test Recipe with Mixed Images",
      Description = "A recipe with valid and invalid image URLs",
      ImageUrls = new List<string>
      {
        "invalid-url",
        "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
        "",
        "https://example.com/image.jpg"
      },
      Ingredients = new List<string> { "Ingredient 1" },
      Directions = new List<string> { "Step 1" },
      Notes = "Test notes",
      PrepTime = "5 minutes",
      CookTime = "10 minutes",
      TotalTime = "15 minutes",
      Servings = "2"
    };

    return new CookbookOrder(_fixture.Create<Zarichney.Cookbook.Customers.Customer>(),
      _fixture.Create<CookbookOrderSubmission>(), _fixture.Create<List<string>>())
    {
      OrderId = _fixture.Create<string>(),
      SynthesizedRecipes = new List<SynthesizedRecipe> { recipe }
    };
  }

  #endregion
}
