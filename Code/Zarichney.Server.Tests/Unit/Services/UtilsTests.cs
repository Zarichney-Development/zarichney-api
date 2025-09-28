using FluentAssertions;
using System.Text.Json;
using Xunit;
using Zarichney.Services;

namespace Zarichney.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class UtilsTests
{
  [Fact]
  public void GenerateId_ReturnsEightCharacterString()
  {
    // Act
    var result = Utils.GenerateId();

    // Assert
    result.Should().NotBeNull()
        .And.HaveLength(8, "because the ID should be exactly 8 characters long");
    result.Should().MatchRegex("^[0-9a-f]{8}$", "because the ID should be the first 8 characters of a GUID");
  }

  [Fact]
  public void GenerateId_ReturnsUniqueIds_OnMultipleCalls()
  {
    // Act
    var id1 = Utils.GenerateId();
    var id2 = Utils.GenerateId();
    var id3 = Utils.GenerateId();

    // Assert
    id1.Should().NotBe(id2, "because each generated ID should be unique");
    id2.Should().NotBe(id3, "because each generated ID should be unique");
    id1.Should().NotBe(id3, "because each generated ID should be unique");
  }

  [Fact]
  public void Deserialize_WithValidJsonString_ReturnsDeserializedObject()
  {
    // Arrange
    var json = """{"name":"test","value":42}""";

    // Act
    var result = Utils.Deserialize<TestDto>(json);

    // Assert
    result.Should().NotBeNull("because the JSON should deserialize successfully");
    result.Name.Should().Be("test", "because the name property should be deserialized correctly");
    result.Value.Should().Be(42, "because the value property should be deserialized correctly");
  }

  [Fact]
  public void Deserialize_WithCaseInsensitiveProperties_DeserializesCorrectly()
  {
    // Arrange
    var json = """{"NAME":"test","VALUE":42}""";

    // Act
    var result = Utils.Deserialize<TestDto>(json);

    // Assert
    result.Should().NotBeNull("because case-insensitive deserialization should work");
    result.Name.Should().Be("test", "because property name matching should be case insensitive");
    result.Value.Should().Be(42, "because property name matching should be case insensitive");
  }

  [Fact]
  public void Deserialize_WithInvalidJson_ThrowsJsonException()
  {
    // Arrange
    var invalidJson = "invalid json content";

    // Act
    Action act = () => Utils.Deserialize<TestDto>(invalidJson);

    // Assert
    act.Should().Throw<JsonException>("because invalid JSON should throw a JsonException");
  }

  [Fact]
  public void Deserialize_WithJsonDocumentInput_ReturnsDeserializedObject()
  {
    // Arrange
    var json = """{"name":"test","value":42}""";
    var jsonDocument = JsonDocument.Parse(json);

    // Act
    var result = Utils.Deserialize<TestDto>(jsonDocument);

    // Assert
    result.Should().NotBeNull("because the JsonDocument should deserialize successfully");
    result.Name.Should().Be("test", "because the name property should be deserialized correctly");
    result.Value.Should().Be(42, "because the value property should be deserialized correctly");
  }

  [Theory]
  [InlineData("HelloWorld", "Hello World")]
  [InlineData("XMLHttpRequest", "X M L Http Request")]
  [InlineData("iPhone", "i Phone")]
  [InlineData("ID", "I D")]
  [InlineData("lowercase", "lowercase")]
  [InlineData("", "")]
  public void SplitCamelCase_WithVariousInputs_ReturnsSplitString(string input, string expected)
  {
    // Act
    var result = Utils.SplitCamelCase(input);

    // Assert
    result.Should().Be(expected, "because the method should split camel case strings correctly");
  }

  [Fact]
  public void GetPropertyValue_WithValidProperty_ReturnsPropertyValue()
  {
    // Arrange
    var obj = new TestDto { Name = "test", Value = 42 };

    // Act
    var result = Utils.GetPropertyValue(obj, nameof(TestDto.Name));

    // Assert
    result.Should().Be("test", "because the property value should be retrieved correctly");
  }

  [Fact]
  public void GetPropertyValue_WithNonExistentProperty_ReturnsEmptyString()
  {
    // Arrange
    var obj = new TestDto { Name = "test", Value = 42 };

    // Act
    var result = Utils.GetPropertyValue(obj, "NonExistentProperty");

    // Assert
    result.Should().BeEmpty("because non-existent properties should return empty string");
  }

  [Fact]
  public void GetListPropertyValue_WithValidListProperty_ReturnsListValues()
  {
    // Arrange
    var obj = new TestDtoWithList { Items = new List<string> { "item1", "item2", "item3" } };

    // Act
    var result = Utils.GetListPropertyValue(obj, nameof(TestDtoWithList.Items));

    // Assert
    result.Should().NotBeNull()
        .And.HaveCount(3, "because the list property should be retrieved correctly")
        .And.ContainInOrder("item1", "item2", "item3");
  }

  [Theory]
  [InlineData("Test Header", 1, "# Test Header\n\n")]
  [InlineData("Sub Header", 2, "## Sub Header\n\n")]
  [InlineData("Deep Header", 6, "###### Deep Header\n\n")]
  [InlineData("", 1, "# \n\n")]
  [InlineData(null, 1, "# \n\n")]
  public void ToMarkdownHeader_WithVariousInputs_ReturnsCorrectMarkdown(string? input, int level, string expected)
  {
    // Act
    var result = Utils.ToMarkdownHeader(input, level);

    // Assert
    result.Should().Be(expected, "because the method should format headers with correct markdown syntax");
  }

  [Theory]
  [InlineData("Alt Text", "https://example.com/image.jpg", "![Alt Text](https://example.com/image.jpg)\n\n")]
  [InlineData("", "https://example.com/image.jpg", "![](https://example.com/image.jpg)\n\n")]
  [InlineData(null, "https://example.com/image.jpg", "![](https://example.com/image.jpg)\n\n")]
  public void ToMarkdownImage_WithVariousInputs_ReturnsCorrectMarkdown(string? altText, string? url, string expected)
  {
    // Act
    var result = Utils.ToMarkdownImage(altText, url);

    // Assert
    result.Should().Be(expected, "because the method should format images with correct markdown syntax");
  }

  [Theory]
  [InlineData("Property", "Value", "**Property:** Value\n")]
  [InlineData("Property", "", "**Property:** \n")]
  [InlineData("Property", null, "**Property:** \n")]
  public void ToMarkdownProperty_WithValidInputs_ReturnsCorrectMarkdown(string? name, string? value, string expected)
  {
    // Act
    var result = Utils.ToMarkdownProperty(name, value);

    // Assert
    result.Should().Be(expected, "because the method should format properties with correct markdown syntax");
  }

  [Fact]
  public void ToMarkdownProperty_WithEmptyName_FormatsCorrectly()
  {
    // Act
    var result = Utils.ToMarkdownProperty("", "Value");

    // Assert
    result.Should().Be("**:** Value\n", "because empty name should still be formatted properly");
  }

  [Fact]
  public void ToMarkdownProperty_WithNullName_FormatsCorrectly()
  {
    // Act
    var result = Utils.ToMarkdownProperty(null, "Value");

    // Assert
    result.Should().Be("**:** Value\n", "because null name should be handled gracefully");
  }

  [Fact]
  public void ToMarkdownList_WithUnorderedList_ReturnsCorrectMarkdown()
  {
    // Arrange
    var items = new[] { "Item 1", "Item 2", "Item 3" };

    // Act
    var result = Utils.ToMarkdownList(items);

    // Assert
    result.Should().Be("- Item 1\n- Item 2\n- Item 3\n\n",
        "because the method should create unordered lists with correct markdown syntax");
  }

  [Fact]
  public void ToMarkdownList_WithOrderedList_ReturnsCorrectMarkdown()
  {
    // Arrange
    var items = new[] { "First", "Second", "Third" };

    // Act
    var result = Utils.ToMarkdownList(items, numbered: true);

    // Assert
    result.Should().Be("1. First\n2. Second\n3. Third\n\n",
        "because the method should create ordered lists with correct markdown syntax");
  }

  [Fact]
  public void ToMarkdownList_WithEmptyList_ReturnsEmptyWithNewlines()
  {
    // Arrange
    var items = Array.Empty<string>();

    // Act
    var result = Utils.ToMarkdownList(items);

    // Assert
    result.Should().Be("\n\n", "because empty lists should still include the trailing newlines");
  }

  [Theory]
  [InlineData("Title", "Content", "## Title\n\nContent\n\n")]
  [InlineData("", "Content", "Content\n\n")]
  [InlineData(null, "Content", "Content\n\n")]
  [InlineData("Title", "", "## Title\n\n\n\n")]
  public void ToMarkdownSection_WithVariousInputs_ReturnsCorrectMarkdown(string? title, string? content, string expected)
  {
    // Act
    var result = Utils.ToMarkdownSection(title, content);

    // Assert
    result.Should().Be(expected, "because the method should format sections with correct markdown syntax");
  }

  [Fact]
  public void ToMarkdownBlockquote_WithMultilineText_ReturnsCorrectMarkdown()
  {
    // Arrange
    var text = "Line 1\nLine 2\nLine 3";

    // Act
    var result = Utils.ToMarkdownBlockquote(text);

    // Assert
    result.Should().Be("> Line 1\n> Line 2\n> Line 3\n\n",
        "because the method should prefix each line with blockquote syntax");
  }

  [Theory]
  [InlineData("console.log('Hello')", "javascript", "```javascript\nconsole.log('Hello')\n```\n\n")]
  [InlineData("SELECT * FROM users", "sql", "```sql\nSELECT * FROM users\n```\n\n")]
  [InlineData("code", "", "```\ncode\n```\n\n")]
  public void ToMarkdownCodeBlock_WithVariousInputs_ReturnsCorrectMarkdown(string code, string language, string expected)
  {
    // Act
    var result = Utils.ToMarkdownCodeBlock(code, language);

    // Assert
    result.Should().Be(expected, "because the method should format code blocks with correct markdown syntax");
  }

  [Theory]
  [InlineData("Example", "https://example.com", "[Example](https://example.com)")]
  [InlineData("", "https://example.com", "[](https://example.com)")]
  [InlineData("Example", "", "[Example]()")]
  public void ToMarkdownLink_WithVariousInputs_ReturnsCorrectMarkdown(string text, string url, string expected)
  {
    // Act
    var result = Utils.ToMarkdownLink(text, url);

    // Assert
    result.Should().Be(expected, "because the method should format links with correct markdown syntax");
  }

  [Fact]
  public void ToMarkdownHorizontalRule_ReturnsCorrectMarkdown()
  {
    // Act
    var result = Utils.ToMarkdownHorizontalRule();

    // Assert
    result.Should().Be("---\n\n", "because the method should return correct horizontal rule markdown");
  }

  [Fact]
  public void ToMarkdownTable_WithRows_ReturnsCorrectMarkdown()
  {
    // Arrange
    var rows = new List<List<string>>
        {
            new List<string> { "Row1Col1", "Row1Col2" },
            new List<string> { "Row2Col1", "Row2Col2" }
        };

    // Act
    var result = Utils.ToMarkdownTable(rows);

    // Assert
    result.Should().Contain("{.tableStyle}", "because the method should include table styling");
    result.Should().Contain("| Row1Col1 | Row1Col2 |", "because the method should format rows correctly");
    result.Should().Contain("| Row2Col1 | Row2Col2 |", "because the method should format rows correctly");
    result.Should().Contain("| --- | --- |", "because the method should include separator row");
  }

  private class TestDto
  {
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
  }

  private class TestDtoWithList
  {
    public List<string> Items { get; set; } = new List<string>();
  }
}

[Trait("Category", "Unit")]
public class ObjectExtensionsTests
{
  [Fact]
  public void ToMarkdown_WithSimpleObject_ReturnsFormattedMarkdown()
  {
    // Arrange
    var obj = new TestObject
    {
      Title = "Test Title",
      Description = "Test Description",
      Count = 42
    };

    // Act
    var result = obj.ToMarkdown("Test");

    // Assert
    result.Should().Contain("## Test", "because the method should include the specified title");
    result.Should().Contain("Title: Test Title", "because the method should format properties");
    result.Should().Contain("Description: Test Description", "because the method should format properties");
    result.Should().Contain("Count: 42", "because the method should format properties");
  }

  [Fact]
  public void ToMarkdown_WithEmptyObject_ReturnsNoContentMessage()
  {
    // Arrange
    var obj = new EmptyTestObject();

    // Act
    var result = obj.ToMarkdown("Empty");

    // Assert
    result.Should().Contain("## Empty", "because the method should include the title");
    result.Should().Contain("No content available", "because empty objects should show appropriate message");
  }

  [Fact]
  public void ToMarkdownHeader_WithValidProperty_ReturnsFormattedHeader()
  {
    // Arrange
    var obj = new TestObject { Title = "Test Title" };

    // Act
    var result = obj.ToMarkdownHeader(nameof(TestObject.Title), 2);

    // Assert
    result.Should().Be("## Test Title\n\n", "because the method should format property values as headers");
  }

  [Fact]
  public void ToMarkdownHeader_WithEmptyProperty_ReturnsEmptyString()
  {
    // Arrange
    var obj = new TestObject { Title = "" };

    // Act
    var result = obj.ToMarkdownHeader(nameof(TestObject.Title));

    // Assert
    result.Should().BeEmpty("because empty properties should return empty strings");
  }

  [Fact]
  public void ToMarkdownImage_WithValidProperties_ReturnsFormattedImage()
  {
    // Arrange
    var obj = new TestObject
    {
      ImageAlt = "Test Image",
      ImageUrl = "https://example.com/image.jpg"
    };

    // Act
    var result = obj.ToMarkdownImage(nameof(TestObject.ImageAlt), nameof(TestObject.ImageUrl));

    // Assert
    result.Should().Be("![Test Image](https://example.com/image.jpg)\n\n",
        "because the method should format image markdown from object properties");
  }

  [Fact]
  public void ToMarkdownProperty_WithValidProperty_ReturnsFormattedProperty()
  {
    // Arrange
    var obj = new TestObject { Title = "Test Title" };

    // Act
    var result = obj.ToMarkdownProperty(nameof(TestObject.Title));

    // Assert
    result.Should().Be("**Title:** Test Title\n",
        "because the method should format properties with split camel case names");
  }

  [Fact]
  public void ToMarkdownList_WithValidListProperty_ReturnsFormattedList()
  {
    // Arrange
    var obj = new TestObjectWithList
    {
      Items = new List<string> { "Item 1", "Item 2", "Item 3" }
    };

    // Act
    var result = obj.ToMarkdownList(nameof(TestObjectWithList.Items));

    // Assert
    result.Should().Contain("## Items", "because the method should include a header for the list");
    result.Should().Contain("- Item 1\n- Item 2\n- Item 3",
        "because the method should format list items correctly");
  }

  [Fact]
  public void ToMarkdownNumberedList_WithValidListProperty_ReturnsFormattedNumberedList()
  {
    // Arrange
    var obj = new TestObjectWithList
    {
      Items = new List<string> { "First", "Second", "Third" }
    };

    // Act
    var result = obj.ToMarkdownNumberedList(nameof(TestObjectWithList.Items));

    // Assert
    result.Should().Contain("## Items", "because the method should include a header for the numbered list");
    result.Should().Contain("1. First\n2. Second\n3. Third",
        "because the method should format numbered list items correctly");
  }

  [Fact]
  public void ToMarkdownSection_WithValidProperty_ReturnsFormattedSection()
  {
    // Arrange
    var obj = new TestObject { Description = "This is a description" };

    // Act
    var result = obj.ToMarkdownSection(nameof(TestObject.Description));

    // Assert
    result.Should().Contain("## Description", "because the method should include a section header");
    result.Should().Contain("This is a description", "because the method should include the property value");
  }

  [Fact]
  public void ToMarkdownSection_WithIncludeTitleFalse_ReturnsContentOnly()
  {
    // Arrange
    var obj = new TestObject { Description = "This is a description" };

    // Act
    var result = obj.ToMarkdownSection(nameof(TestObject.Description), includeTitle: false);

    // Assert
    result.Should().NotContain("## Description", "because the title should not be included when includeTitle is false");
    result.Should().Contain("This is a description", "because the method should still include the property value");
  }

  private class TestObject
  {
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Count { get; set; }
    public string ImageAlt { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
  }

  private class EmptyTestObject
  {
    public string EmptyString { get; set; } = string.Empty;
    public int DefaultInt { get; set; } = 0;
  }

  private class TestObjectWithList
  {
    public List<string> Items { get; set; } = new List<string>();
  }
}

[Trait("Category", "Unit")]
public class AtomicCounterTests
{
  [Fact]
  public void Increment_ReturnsIncrementedValue()
  {
    // Arrange
    var counter = new AtomicCounter();

    // Act
    var result1 = counter.Increment();
    var result2 = counter.Increment();
    var result3 = counter.Increment();

    // Assert
    result1.Should().Be(1, "because the first increment should return 1");
    result2.Should().Be(2, "because the second increment should return 2");
    result3.Should().Be(3, "because the third increment should return 3");
  }

  [Fact]
  public async Task Increment_IsThreadSafe()
  {
    // Arrange
    var counter = new AtomicCounter();
    var tasks = new List<Task<int>>();
    const int numTasks = 100;

    // Act
    for (int i = 0; i < numTasks; i++)
    {
      tasks.Add(Task.Run(() => counter.Increment()));
    }

    var results = await Task.WhenAll(tasks);

    // Assert
    results.Should().HaveCount(numTasks, "because all tasks should complete");
    results.Should().OnlyHaveUniqueItems("because atomic increment should ensure unique values");
    results.Max().Should().Be(numTasks, "because the maximum value should equal the number of tasks");
  }
}

[Trait("Category", "Unit")]
public class HtmlStripperTests
{
  [Theory]
  [InlineData("<p>Hello World</p>", "Hello World")]
  [InlineData("<div>Content</div>", "Content")]
  [InlineData("<span>Text</span>", "Text")]
  [InlineData("", "")]
  [InlineData(null, "")]
  public void StripHtml_WithBasicHtml_RemovesTagsCorrectly(string? input, string expected)
  {
    // Act
    var result = HtmlStripper.StripHtml(input!);

    // Assert
    result.Should().Be(expected, "because the method should strip HTML tags while preserving content");
  }

  [Fact]
  public void StripHtml_WithBreakTags_ConvertsToNewlines()
  {
    // Arrange
    var html = "Line 1<br>Line 2<br/>Line 3";

    // Act
    var result = HtmlStripper.StripHtml(html);

    // Assert
    result.Should().Be("Line 1\nLine 2\nLine 3",
        "because <br> tags should be converted to newlines");
  }

  [Fact]
  public void StripHtml_WithHeadingTags_ConvertsToUppercaseWithNewlines()
  {
    // Arrange
    var html = "<h1>Main Title</h1><h2>Sub Title</h2>";

    // Act
    var result = HtmlStripper.StripHtml(html);

    // Assert
    result.Should().Contain("MAIN TITLE", "because h1 content should be converted to uppercase");
    result.Should().Contain("SUB TITLE", "because h2 content should be converted to uppercase");
  }

  [Fact]
  public void StripHtml_WithListItems_ConvertsToBulletPoints()
  {
    // Arrange
    var html = "<ul><li>Item 1</li><li>Item 2</li></ul>";

    // Act
    var result = HtmlStripper.StripHtml(html);

    // Assert
    result.Should().Contain("• Item 1", "because <li> should be converted to bullet points");
    result.Should().Contain("• Item 2", "because <li> should be converted to bullet points");
  }

  [Fact]
  public void StripHtml_WithHtmlEntities_DecodesCorrectly()
  {
    // Arrange
    var html = "&lt;Hello&gt; &amp; &quot;World&quot;";

    // Act
    var result = HtmlStripper.StripHtml(html);

    // Assert
    result.Should().Be("<Hello> & \"World\"",
        "because HTML entities should be decoded properly");
  }

  [Fact]
  public void StripHtml_WithComplexHtml_CleansCorrectly()
  {
    // Arrange
    var html = """
            <div class="container">
                <h1>Title</h1>
                <p>This is a <strong>paragraph</strong> with <em>emphasis</em>.</p>
                <ul>
                    <li>Item 1</li>
                    <li>Item 2</li>
                </ul>
                <script>alert('malicious');</script>
            </div>
            """;

    // Act
    var result = HtmlStripper.StripHtml(html);

    // Assert
    result.Should().Contain("TITLE", "because headings should be converted to uppercase");
    result.Should().Contain("This is a paragraph with emphasis", "because paragraph content should be preserved");
    result.Should().Contain("• Item 1", "because list items should be converted to bullets");
    result.Should().Contain("• Item 2", "because list items should be converted to bullets");
    result.Should().NotContain("script", "because script tags should be completely removed");
    result.Should().NotContain("alert", "because script content should be removed");
  }

  [Fact]
  public void StripHtml_WithNonBreakingSpaces_CleansCorrectly()
  {
    // Arrange
    var html = "Text&nbsp;&nbsp;&nbsp;with&nbsp;spaces";

    // Act
    var result = HtmlStripper.StripHtml(html);

    // Assert
    result.Should().Be("Textwithspaces", "because non-breaking spaces should be cleaned up");
  }
}
