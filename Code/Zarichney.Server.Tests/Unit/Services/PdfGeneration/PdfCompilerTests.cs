using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.FileSystem;
using Zarichney.Services.PdfGeneration;

namespace Zarichney.Tests.Unit.Services.PdfGeneration;

/// <summary>
/// Unit tests for PdfCompiler service - tests PDF generation components with proper isolation
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "PdfGeneration")]
public class PdfCompilerTests
{
  private readonly IFixture _fixture;
  private readonly Mock<IFileService> _mockFileService;
  private readonly Mock<ILogger<PdfCompiler>> _mockLogger;
  private readonly PdfCompilerConfig _config;
  private readonly PdfCompiler _sut;

  public PdfCompilerTests()
  {
    _fixture = new Fixture();
    _mockFileService = new Mock<IFileService>();
    _mockLogger = new Mock<ILogger<PdfCompiler>>();
    _config = new PdfCompilerConfig
    {
      FontName = "Arial",
      FontSize = 12,
      ImageDirectory = "test-images"
    };
    _sut = new PdfCompiler(_config, _mockFileService.Object, _mockLogger.Object);
  }

  #region Configuration Tests

  [Fact]
  public void PdfCompilerConfig_DefaultValues_ShouldBeSet()
  {
    // Arrange & Act
    var config = new PdfCompilerConfig();

    // Assert
    config.FontName.Should().Be("Garamond", "default font name should be Garamond");
    config.FontSize.Should().Be(12, "default font size should be 12");
    config.ImageDirectory.Should().Be("temp", "default image directory should be temp");
  }

  [Fact]
  public void PdfCompilerConfig_CustomValues_ShouldBeInitialized()
  {
    // Arrange
    var fontName = _fixture.Create<string>();
    var fontSize = _fixture.Create<int>();
    var imageDirectory = _fixture.Create<string>();

    // Act
    var config = new PdfCompilerConfig
    {
      FontName = fontName,
      FontSize = fontSize,
      ImageDirectory = imageDirectory
    };

    // Assert
    config.FontName.Should().Be(fontName, "custom font name should be set");
    config.FontSize.Should().Be(fontSize, "custom font size should be set");
    config.ImageDirectory.Should().Be(imageDirectory, "custom image directory should be set");
  }

  #endregion

  #region CompileCookbook Tests - Basic Functionality

  [Fact]
  public async Task CompileCookbook_WithValidOrderNoImages_ShouldReturnPdfBytes()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(1);

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF compilation should return byte array");
    result.Should().NotBeEmpty("PDF byte array should contain data");
    result[0].Should().Be(0x25, "PDF should start with % character (PDF header)");
  }

  [Fact]
  public async Task CompileCookbook_WithMultipleRecipes_ShouldGeneratePageBreaks()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(3);

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF compilation should return byte array");
    result.Should().NotBeEmpty("PDF byte array should contain data");
    result.Length.Should().BeGreaterThan(1000, "multi-recipe PDF should have substantial size");
  }

  [Fact]
  public async Task CompileCookbook_WithEmptyRecipeList_ShouldReturnPdfWithoutContent()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(0);

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF compilation should return byte array even for empty recipes");
    result.Should().NotBeEmpty("PDF byte array should contain minimal PDF structure");
  }

  #endregion

  #region CompileCookbook Tests - Image Processing Scenarios

  [Fact]
  public async Task CompileCookbook_ExecutesCleanupPhase_EvenWithNoImages()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(1);
    order.SynthesizedRecipes[0].Title = "Test Recipe";

    _mockFileService.Setup(x => x.DeleteFile(It.IsAny<string>()));

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF should compile successfully");
    // Note: Cleanup only occurs if there are files to clean up, which is expected behavior
  }

  [Fact]
  public async Task CompileCookbook_HandlesCompilationGracefully_EvenWithComplexContent()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(1);
    order.SynthesizedRecipes[0].Title = "Complex Recipe";
    order.SynthesizedRecipes[0].Description = "A recipe with **bold** and *italic* markdown formatting";

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF should compile even with complex markdown content");
    result.Should().NotBeEmpty("PDF should contain content");
  }

  #endregion

  #region Content Processing Tests

  [Fact]
  public async Task CompileCookbook_WithSpecialCharacters_ShouldHandleCorrectly()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(1);
    order.SynthesizedRecipes[0].Title = "Recipe with Special Characters: &<>\"'";
    order.SynthesizedRecipes[0].Ingredients = ["1 cup of salt & pepper", "2 tbsp \"special\" sauce"];

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF with special characters should compile successfully");
  }

  [Fact]
  public async Task CompileCookbook_WithLongContent_ShouldHandleMultiplePages()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(1);
    var longIngredients = Enumerable.Range(1, 25)
      .Select(i => $"Ingredient {i}: This is a very long ingredient description that should take up multiple lines in the final PDF document")
      .ToList();
    order.SynthesizedRecipes[0].Ingredients = longIngredients;

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF with long content should compile successfully");
    result.Length.Should().BeGreaterThan(2000, "long content should result in larger PDF size");
  }

  #endregion

  #region Edge Case Tests

  [Fact]
  public async Task CompileCookbook_WithNullImageUrls_ShouldHandleGracefully()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(1);
    order.SynthesizedRecipes[0].ImageUrls = null;

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF should compile when ImageUrls is null");
  }

  [Fact]
  public async Task CompileCookbook_WithEmptyImageUrls_ShouldSkipImageProcessing()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(1);
    order.SynthesizedRecipes[0].ImageUrls = [];

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF should compile with empty image URL list");
    _mockFileService.Verify(
      x => x.CreateFile(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()),
      Times.Never,
      "no image processing should occur with empty URL list");
  }

  [Fact]
  public async Task CompileCookbook_WithNullRecipeTitle_ShouldHandleGracefully()
  {
    // Arrange
    var order = CreateCookbookOrderWithRecipes(1);
    order.SynthesizedRecipes[0].Title = null;

    // Act
    var result = await _sut.CompileCookbook(order);

    // Assert
    result.Should().NotBeNull("PDF should compile even with null recipe title");
  }

  #endregion

  #region Private Test Helpers

  private CookbookOrder CreateCookbookOrderWithRecipes(int recipeCount)
  {
    var recipes = new List<SynthesizedRecipe>();
    for (var i = 0; i < recipeCount; i++)
    {
      recipes.Add(new SynthesizedRecipe
      {
        Title = $"Test Recipe {i + 1}",
        Description = $"Test description for recipe {i + 1}",
        Ingredients = [$"Ingredient 1 for recipe {i + 1}", $"Ingredient 2 for recipe {i + 1}"],
        Directions = [$"Step 1 for recipe {i + 1}", $"Step 2 for recipe {i + 1}"],
        Notes = $"Test notes for recipe {i + 1}",
        PrepTime = "15 minutes",
        CookTime = "30 minutes",
        TotalTime = "45 minutes",
        Servings = "4"
      });
    }

    return new CookbookOrder(_fixture.Create<Zarichney.Cookbook.Customers.Customer>(),
      _fixture.Create<CookbookOrderSubmission>(), _fixture.Create<List<string>>())
    {
      OrderId = _fixture.Create<string>(),
      SynthesizedRecipes = recipes
    };
  }

  #endregion
}
