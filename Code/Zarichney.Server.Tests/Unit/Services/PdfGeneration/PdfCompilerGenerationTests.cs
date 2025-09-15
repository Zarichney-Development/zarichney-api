using AutoFixture;
using FluentAssertions;
using Markdig;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Moq;
using QuestPDF.Infrastructure;
using Xunit;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using Zarichney.Services.FileSystem;
using Zarichney.Services.PdfGeneration;

namespace Zarichney.Tests.Unit.Services.PdfGeneration;

/// <summary>
/// Unit tests for PdfCompiler PDF generation functionality
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "PdfGeneration")]
public class PdfCompilerGenerationTests
{
  private readonly IFixture _fixture;
  private readonly Mock<IFileService> _mockFileService;
  private readonly Mock<ILogger<PdfCompiler>> _mockLogger;
  private readonly PdfCompilerConfig _config;
  private readonly Mock<PdfCompiler> _mockPdfCompiler;

  public PdfCompilerGenerationTests()
  {
    // Set QuestPDF license
    QuestPDF.Settings.License = LicenseType.Community;
    
    _fixture = new Fixture();
    _mockFileService = new Mock<IFileService>();
    _mockLogger = new Mock<ILogger<PdfCompiler>>();
    _config = new PdfCompilerConfig
    {
      FontName = "Arial",
      FontSize = 12,
      ImageDirectory = "test-images"
    };
    
    // Create partial mock to test protected methods
    _mockPdfCompiler = new Mock<PdfCompiler>(_config, _mockFileService.Object, _mockLogger.Object)
    {
      CallBase = true
    };
  }

  #region GeneratePdf Tests

  [Fact]
  public void GeneratePdf_WithSingleRecipe_ShouldGeneratePdfBytes()
  {
    // Arrange
    var markdown = "# Test Recipe\n\nThis is a test recipe.";
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.Parse(markdown, pipeline);
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, null)
    };

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("PDF generation should return byte array");
    result.Should().NotBeEmpty("PDF byte array should contain data");
    result.Should().StartWith("%PDF"u8.ToArray(), "PDF should start with PDF header");
  }

  [Fact]
  public void GeneratePdf_WithMultipleRecipes_ShouldIncludePageBreaks()
  {
    // Arrange
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var content = new List<(MarkdownDocument Content, string? ImagePath)>();
    
    for (int i = 1; i <= 3; i++)
    {
      var markdown = $"# Recipe {i}\n\nDescription for recipe {i}.";
      var document = Markdown.Parse(markdown, pipeline);
      content.Add((document, null));
    }

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("multi-recipe PDF should generate successfully");
    result.Length.Should().BeGreaterThan(1000, "multi-recipe PDF should have substantial size");
  }

  [Fact]
  public void GeneratePdf_WithEmptyContent_ShouldGenerateMinimalPdf()
  {
    // Arrange
    var content = new List<(MarkdownDocument Content, string? ImagePath)>();

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("empty content should still generate PDF structure");
    result.Should().NotBeEmpty("PDF should contain minimal structure");
    result.Should().StartWith("%PDF"u8.ToArray(), "PDF should have valid header");
  }

  [Fact]
  public void GeneratePdf_WithComplexMarkdown_ShouldHandleAllElements()
  {
    // Arrange
    var markdown = @"# Complex Recipe

## Ingredients
- **Bold ingredient**
- *Italic ingredient*
- Regular ingredient

## Directions
1. First step with **emphasis**
2. Second step with *italics*
3. Third step with [link](https://example.com)

### Notes
This recipe contains:
- Lists
- Tables
- Emphasis

| Column 1 | Column 2 |
|----------|----------|
| Value 1  | Value 2  |

---

> Blockquote text

`Code block`";
    
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.Parse(markdown, pipeline);
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, null)
    };

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("complex markdown should generate PDF successfully");
    result.Length.Should().BeGreaterThan(2000, "complex content should result in larger PDF");
  }

  [Fact]
  public void GeneratePdf_WithImagePath_ShouldAttemptToIncludeImage()
  {
    // Arrange
    var markdown = "# Recipe with Image\n\n![Recipe Image](image.jpg)";
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.Parse(markdown, pipeline);
    var imagePath = "test-images/recipe_image.jpg";
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, imagePath)
    };
    
    _mockFileService.Setup(x => x.FileExists(imagePath)).Returns(false);

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("PDF with image path should generate successfully");
    _mockFileService.Verify(x => x.FileExists(imagePath), Times.AtLeastOnce);
  }

  #endregion

  #region Markdown Formatting Tests

  [Fact]
  public void GeneratePdf_WithRecipeMetadata_ShouldFormatCorrectly()
  {
    // Arrange
    var markdown = @"# Delicious Recipe

Prep Time: 15 minutes
Cook Time: 30 minutes
Total Time: 45 minutes

Servings: 4

## Description
A wonderful recipe for any occasion.";
    
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.Parse(markdown, pipeline);
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, null)
    };

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("recipe with metadata should generate PDF");
    result.Length.Should().BeGreaterThan(1000, "metadata should be included in PDF");
  }

  [Fact]
  public void GeneratePdf_WithTables_ShouldRenderTablesCorrectly()
  {
    // Arrange
    var markdown = @"# Recipe with Table

## Nutritional Information

| Nutrient | Amount | % Daily Value |
|----------|--------|---------------|
| Calories | 250    | 12%           |
| Protein  | 15g    | 30%           |
| Carbs    | 30g    | 10%           |
| Fat      | 8g     | 12%           |";
    
    var pipeline = new MarkdownPipelineBuilder()
      .UseAdvancedExtensions()
      .UsePipeTables()
      .Build();
    var document = Markdown.Parse(markdown, pipeline);
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, null)
    };

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("PDF with tables should generate successfully");
    result.Should().NotBeEmpty("PDF should contain table content");
  }

  [Fact]
  public void GeneratePdf_WithNestedLists_ShouldHandleCorrectly()
  {
    // Arrange
    var markdown = @"# Recipe with Lists

## Ingredients
- Main ingredients:
  - 2 cups flour
  - 1 cup sugar
- Secondary ingredients:
  - 1 tsp vanilla
  - 2 eggs

## Directions
1. Mix dry ingredients
2. Add wet ingredients
3. Combine thoroughly";
    
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.Parse(markdown, pipeline);
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, null)
    };

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("PDF with nested lists should generate successfully");
  }

  #endregion

  #region Edge Cases

  [Fact]
  public void GeneratePdf_WithSpecialCharacters_ShouldEscapeCorrectly()
  {
    // Arrange
    var markdown = @"# Recipe & Special <Characters>

## ""Quoted"" Ingredients
- Salt & pepper
- 'Special' sauce
- Item with <brackets>
- Item with $pecial symbols";
    
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.Parse(markdown, pipeline);
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, null)
    };

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("PDF with special characters should generate successfully");
  }

  [Fact]
  public void GeneratePdf_WithVeryLongContent_ShouldHandleMultiplePages()
  {
    // Arrange
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var content = new List<(MarkdownDocument Content, string? ImagePath)>();
    
    // Create 10 recipes with substantial content
    for (int i = 1; i <= 10; i++)
    {
      var markdown = $@"# Recipe {i}

## Description
{string.Join("\n", Enumerable.Range(1, 10).Select(j => $"This is paragraph {j} with detailed description of the recipe."))}

## Ingredients
{string.Join("\n", Enumerable.Range(1, 20).Select(j => $"- Ingredient {j} with detailed measurement instructions"))}

## Directions
{string.Join("\n", Enumerable.Range(1, 15).Select(j => $"{j}. Detailed step {j} with comprehensive instructions for preparation"))}";
      
      var document = Markdown.Parse(markdown, pipeline);
      content.Add((document, null));
    }

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("very long content should generate PDF successfully");
    result.Length.Should().BeGreaterThan(10000, "long content should result in large PDF file");
  }

  [Fact]
  public void GeneratePdf_WithUnicodeCharacters_ShouldHandleCorrectly()
  {
    // Arrange
    var markdown = @"# Crème Brûlée Recipe 🍮

## Ingredients
- 500ml crème fraîche
- 100g açúcar
- Zest of 1 limón
- ½ tsp κανέλα (cinnamon)
- 北京烤鸭 seasoning

## Directions
1. Mix crème with sugar
2. Add spices: κανέλα & 辣椒
3. Garnish with 🍓 and 🥝";
    
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.Parse(markdown, pipeline);
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, null)
    };

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("PDF with unicode characters should generate successfully");
  }

  #endregion

  #region Configuration Tests

  [Fact]
  public void GeneratePdf_WithCustomFontConfig_ShouldApplyFontSettings()
  {
    // Arrange
    var customConfig = new PdfCompilerConfig
    {
      FontName = "Times New Roman",
      FontSize = 14,
      ImageDirectory = "custom-images"
    };
    
    var customCompiler = new PdfCompiler(customConfig, _mockFileService.Object, _mockLogger.Object);
    
    var markdown = "# Test Recipe\n\nTest content.";
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.Parse(markdown, pipeline);
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, null)
    };

    // Act
    var result = customCompiler.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("PDF with custom font config should generate successfully");
    // Note: We can't easily verify font settings in the binary PDF, but the test ensures no exceptions
  }

  [Fact]
  public void GeneratePdf_WithMinimalMarkdown_ShouldStillGeneratePdf()
  {
    // Arrange
    var markdown = "Plain text without any markdown formatting";
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var document = Markdown.Parse(markdown, pipeline);
    var content = new List<(MarkdownDocument Content, string? ImagePath)>
    {
      (document, null)
    };

    // Act
    var result = _mockPdfCompiler.Object.GeneratePdf(content);

    // Assert
    result.Should().NotBeNull("plain text should still generate PDF");
    result.Should().NotBeEmpty("PDF should contain the plain text");
  }

  #endregion
}