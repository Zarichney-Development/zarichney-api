using FluentAssertions;
using System.Text.Json;
using Xunit;
using Zarichney.Services;

namespace Zarichney.Tests.Unit.Services;

/// <summary>
/// Extended test cases for Utils class covering additional edge cases,
/// error scenarios, and complex interactions between methods.
/// </summary>
[Trait("Category", "Unit")]
public class UtilsExtendedTests
{
  #region Deserialize Tests - Additional Edge Cases

  [Fact]
  public void Deserialize_WithFieldsIncluded_DeserializesFieldsCorrectly()
  {
    // Arrange
    var json = """{"publicField":"fieldValue","Name":"test"}""";

    // Act
    var result = Utils.Deserialize<TestDtoWithFields>(json);

    // Assert
    result.Should().NotBeNull("because deserialization should succeed");
    result.PublicField.Should().Be("fieldValue", "because fields should be included in deserialization");
    result.Name.Should().Be("test", "because properties should still work");
  }

  [Theory]
  [InlineData("""{"nested":{"value":42}}""", 42)]
  [InlineData("""{"nested":null}""", 0)]
  [InlineData("""{}""", 0)]
  public void Deserialize_WithNestedObjects_HandlesCorrectly(string json, int expectedValue)
  {
    // Act
    var result = Utils.Deserialize<TestDtoWithNested>(json);

    // Assert
    result.Should().NotBeNull("because deserialization should always return an object");
    if (result.Nested != null)
    {
      result.Nested.Value.Should().Be(expectedValue);
    }
    else
    {
      expectedValue.Should().Be(0, "because null nested objects should match expected");
    }
  }

  [Fact]
  public void Deserialize_JsonDocument_WithComplexStructure_DeserializesCorrectly()
  {
    // Arrange
    var json = """
      {
        "items": ["item1", "item2"],
        "metadata": {
          "count": 2,
          "timestamp": "2024-01-01T00:00:00Z"
        }
      }
      """;
    using var jsonDocument = JsonDocument.Parse(json);

    // Act
    var result = Utils.Deserialize<ComplexDto>(jsonDocument);

    // Assert
    result.Should().NotBeNull();
    result.Items.Should().HaveCount(2);
    result.Metadata.Should().NotBeNull();
    result.Metadata!.Count.Should().Be(2);
  }

  [Fact]
  public void Deserialize_WithExtraProperties_IgnoresUnknownFields()
  {
    // Arrange
    var json = """{"name":"test","value":42,"unknownField":"ignored"}""";

    // Act
    var result = Utils.Deserialize<SimpleTestDto>(json);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("test");
    result.Value.Should().Be(42);
    // Unknown field should be silently ignored
  }

  #endregion

  #region GetPropertyValue and GetListPropertyValue Tests

  [Fact]
  public void GetPropertyValue_WithInheritedProperty_ReturnsValue()
  {
    // Arrange
    var obj = new DerivedDto { BaseProp = "base", DerivedProp = "derived" };

    // Act
    var baseResult = Utils.GetPropertyValue(obj, nameof(DerivedDto.BaseProp));
    var derivedResult = Utils.GetPropertyValue(obj, nameof(DerivedDto.DerivedProp));

    // Assert
    baseResult.Should().Be("base", "because inherited properties should be accessible");
    derivedResult.Should().Be("derived", "because derived properties should be accessible");
  }

  [Theory]
  [InlineData(42, "42")]
  [InlineData(3.14, "3.14")]
  [InlineData(true, "True")]
  [InlineData(false, "False")]
  public void GetPropertyValue_WithValueTypes_ConvertsToString(object value, string expected)
  {
    // Arrange
    var obj = new ValueTypeDto { IntValue = 42, DoubleValue = 3.14, BoolValue = value is bool b ? b : true };
    var propertyName = value switch
    {
      int => nameof(ValueTypeDto.IntValue),
      double => nameof(ValueTypeDto.DoubleValue),
      bool => nameof(ValueTypeDto.BoolValue),
      _ => throw new InvalidOperationException()
    };

    // Act
    var result = Utils.GetPropertyValue(obj, propertyName);

    // Assert
    result.Should().Be(expected, "because value types should convert to strings correctly");
  }

  [Fact]
  public void GetListPropertyValue_WithIEnumerableImplementations_ReturnsCorrectly()
  {
    // Arrange
    var obj = new CollectionDto
    {
      StringArray = new[] { "a", "b", "c" },
      StringList = new List<string> { "d", "e", "f" },
      StringHashSet = new HashSet<string> { "g", "h", "i" }
    };

    // Act
    var arrayResult = Utils.GetListPropertyValue(obj, nameof(CollectionDto.StringArray));
    var listResult = Utils.GetListPropertyValue(obj, nameof(CollectionDto.StringList));
    var hashSetResult = Utils.GetListPropertyValue(obj, nameof(CollectionDto.StringHashSet));

    // Assert
    arrayResult.Should().BeEquivalentTo(new[] { "a", "b", "c" }, "because arrays should work");
    listResult.Should().BeEquivalentTo(new[] { "d", "e", "f" }, "because lists should work");
    hashSetResult.Should().NotBeEmpty("because hash sets should work");
  }

  #endregion

  #region SplitCamelCase Tests - Complex Cases

  [Theory]
  [InlineData("IOError", "I O Error")]
  [InlineData("HTTPSConnection", "H T T P S Connection")]
  [InlineData("getHTTPResponseCode", "get H T T P Response Code")]
  [InlineData("HTTPSConnectionHTTPSConnection", "H T T P S Connection H T T P S Connection")]
  [InlineData("aB", "a B")]
  [InlineData("A", "A")]
  [InlineData("a", "a")]
  public void SplitCamelCase_WithComplexAcronyms_SplitsCorrectly(string input, string expected)
  {
    // Act
    var result = Utils.SplitCamelCase(input);

    // Assert
    result.Should().Be(expected, "because complex acronym patterns should be handled");
  }

  #endregion

  #region Markdown Methods - Edge Cases

  [Fact]
  public void ToMarkdownHeader_WithNullText_HandlesGracefully()
  {
    // Act
    var result = Utils.ToMarkdownHeader(null, 3);

    // Assert
    result.Should().Be("### \n\n", "because null should be treated as empty");
  }

  [Theory]
  [InlineData(0, " Test")]  // Level 0 creates no hashes
  [InlineData(7, "####### Test")]  // Should create 7 hashes
  [InlineData(10, "########## Test")]  // Should create 10 hashes
  public void ToMarkdownHeader_WithInvalidLevels_HandlesGracefully(int level, string expectedStart)
  {
    // Act
    var result = Utils.ToMarkdownHeader("Test", level);

    // Assert
    result.Should().StartWith(expectedStart, "because various levels should produce corresponding hashes");
  }

  [Fact]
  public void ToMarkdownProperty_WithSpecialCharacters_HandlesCorrectly()
  {
    // Act
    var result = Utils.ToMarkdownProperty("Property*With*Asterisks", "Value_With_Underscores");

    // Assert
    result.Should().Be("**Property*With*Asterisks:** Value_With_Underscores\n",
        "because special markdown characters should be preserved literally");
  }

  [Fact]
  public void ToMarkdownList_WithNullItems_HandlesGracefully()
  {
    // Arrange
    var items = new string[] { "First", null!, "Third" };

    // Act
    var result = Utils.ToMarkdownList(items);

    // Assert
    result.Should().Be("- First\n- \n- Third\n\n",
        "because null items should render as empty list items");
  }

  [Fact]
  public void ToMarkdownSection_WithNullContent_HandlesGracefully()
  {
    // Act
    var result = Utils.ToMarkdownSection("Title", null);

    // Assert
    result.Should().Be("## Title\n\n\n\n",
        "because null content should still produce section structure");
  }

  [Fact]
  public void ToMarkdownCodeBlock_WithTripleBackticks_HandlesCorrectly()
  {
    // Arrange
    var code = "```nested\ncode\n```";

    // Act
    var result = Utils.ToMarkdownCodeBlock(code, "javascript");

    // Assert
    result.Should().Be("```javascript\n```nested\ncode\n```\n```\n\n",
        "because nested code blocks should be preserved literally");
  }

  [Fact]
  public void ToMarkdownTable_WithJaggedRows_HandlesCorrectly()
  {
    // Arrange
    var rows = new List<List<string>>
        {
            new List<string> { "A" },
            new List<string> { "B", "C" },
            new List<string> { "D", "E", "F" }
        };

    // Act
    var result = Utils.ToMarkdownTable(rows);

    // Assert
    result.Should().Contain("| A |", "because single-column rows should work");
    result.Should().Contain("| B | C |", "because two-column rows should work");
    result.Should().Contain("| D | E | F |", "because three-column rows should work");
  }

  [Fact]
  public void ToMarkdownTable_WithPipeCharactersInContent_DoesNotEscape()
  {
    // Arrange
    var rows = new List<List<string>>
        {
            new List<string> { "A|B", "C|D" },
            new List<string> { "E|F", "G|H" }
        };

    // Act
    var result = Utils.ToMarkdownTable(rows);

    // Assert
    // Note: This might break markdown table rendering if pipes aren't escaped
    result.Should().Contain("| A|B | C|D |",
        "because pipe characters in content are not escaped (potential issue)");
  }

  #endregion

  #region AtomicCounter - Additional Tests

  [Fact]
  public void AtomicCounter_StartsAtZero()
  {
    // Arrange
    var counter = new AtomicCounter();

    // Act
    var firstValue = counter.Increment();

    // Assert
    firstValue.Should().Be(1, "because counter should start at 0 and increment to 1");
  }

  [Fact]
  public void AtomicCounter_CanIncrementMillionTimes()
  {
    // Arrange
    var counter = new AtomicCounter();
    const int iterations = 1_000_000;

    // Act
    int finalValue = 0;
    for (int i = 0; i < iterations; i++)
    {
      finalValue = counter.Increment();
    }

    // Assert
    finalValue.Should().Be(iterations, "because counter should handle many increments");
  }

  [Fact]
  public async Task AtomicCounter_WithHighContention_MaintainsCorrectness()
  {
    // Arrange
    var counter = new AtomicCounter();
    const int threadsCount = 50;
    const int incrementsPerThread = 1000;
    var allValues = new System.Collections.Concurrent.ConcurrentBag<int>();

    // Act
    var tasks = Enumerable.Range(0, threadsCount).Select(_ => Task.Run(() =>
    {
      for (int i = 0; i < incrementsPerThread; i++)
      {
        allValues.Add(counter.Increment());
      }
    }));

    await Task.WhenAll(tasks);

    // Assert
    allValues.Should().HaveCount(threadsCount * incrementsPerThread);
    allValues.Max().Should().Be(threadsCount * incrementsPerThread,
        "because the maximum value should equal total increments");
    allValues.Distinct().Count().Should().Be(threadsCount * incrementsPerThread,
        "because all values should be unique");
  }

  #endregion

  #region ObjectExtensions - Complex Scenarios

  [Fact]
  public void ToMarkdown_WithCircularReference_ShouldNotCauseStackOverflow()
  {
    // Arrange
    var obj = new CircularDto();
    obj.Self = obj; // Create circular reference

    // Act & Assert
    // This might cause issues if not handled - documenting behavior
    Action act = () => obj.ToMarkdown("Circular");

    // The current implementation doesn't handle circular references
    // It would need cycle detection to prevent stack overflow
    act.Should().NotThrow("because circular references should be handled gracefully");
  }

  [Fact]
  public void ToMarkdown_WithComplexNestedObject_FormatsCorrectly()
  {
    // Arrange
    var obj = new ComplexNestedDto
    {
      Level1 = "Top",
      Nested = new NestedLevel
      {
        Level2 = "Middle",
        DeepNested = new DeepNestedLevel
        {
          Level3 = "Bottom"
        }
      }
    };

    // Act
    var result = obj.ToMarkdown("Complex");

    // Assert
    result.Should().Contain("## Complex");
    result.Should().Contain("Level 1: Top");
    // ToString() returns the type name by default
    result.Should().Contain("Nested: Zarichney.Tests.Unit.Services.UtilsExtendedTests+NestedLevel");
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("\t")]
  [InlineData("\n")]
  public void ToMarkdownProperty_WithWhitespaceValues_HandlesCorrectly(string value)
  {
    // Arrange
    var obj = new SimpleTestDto { Name = value };

    // Act
    var result = obj.ToMarkdownProperty(nameof(SimpleTestDto.Name));

    // Assert
    if (string.IsNullOrWhiteSpace(value))
    {
      result.Should().BeEmpty("because whitespace-only values should not generate output");
    }
  }

  #endregion

  // Test DTOs
  private class SimpleTestDto
  {
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
  }

  private class TestDtoWithFields
  {
    public string PublicField = string.Empty;
    public string Name { get; set; } = string.Empty;
  }

  private class TestDtoWithNested
  {
    public NestedDto? Nested { get; set; }
  }

  private class NestedDto
  {
    public int Value { get; set; }
  }

  private class ComplexDto
  {
    public List<string> Items { get; set; } = new();
    public MetadataDto? Metadata { get; set; }
  }

  private class MetadataDto
  {
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
  }

  private class BaseDto
  {
    public string BaseProp { get; set; } = string.Empty;
  }

  private class DerivedDto : BaseDto
  {
    public string DerivedProp { get; set; } = string.Empty;
  }

  private class ValueTypeDto
  {
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
    public bool BoolValue { get; set; }
  }

  private class CollectionDto
  {
    public string[] StringArray { get; set; } = Array.Empty<string>();
    public List<string> StringList { get; set; } = new();
    public HashSet<string> StringHashSet { get; set; } = new();
  }

  private class CircularDto
  {
    public string Name { get; set; } = "Circular";
    public CircularDto? Self { get; set; }
  }

  private class ComplexNestedDto
  {
    public string Level1 { get; set; } = string.Empty;
    public NestedLevel? Nested { get; set; }
  }

  private class NestedLevel
  {
    public string Level2 { get; set; } = string.Empty;
    public DeepNestedLevel? DeepNested { get; set; }
  }

  private class DeepNestedLevel
  {
    public string Level3 { get; set; } = string.Empty;
  }
}