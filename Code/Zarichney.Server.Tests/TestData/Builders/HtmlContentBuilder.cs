using System.Text;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Builder for creating various HTML content scenarios for testing HTML processing utilities.
/// Provides fluent interface for constructing complex HTML structures with predictable content.
/// </summary>
public class HtmlContentBuilder
{
  private readonly StringBuilder _html = new StringBuilder();
  private readonly List<string> _expectedPlainText = new List<string>();

  public HtmlContentBuilder WithHeading(int level, string content)
  {
    if (level < 1 || level > 6)
      throw new ArgumentException("Heading level must be between 1 and 6", nameof(level));

    _html.Append($"<h{level}>{content}</h{level}>");
    _expectedPlainText.Add(content.ToUpper()); // HtmlStripper converts headings to uppercase
    return this;
  }

  public HtmlContentBuilder WithParagraph(string content)
  {
    _html.Append($"<p>{content}</p>");
    _expectedPlainText.Add(content);
    return this;
  }

  public HtmlContentBuilder WithDiv(string content, string? cssClass = null)
  {
    var classAttr = cssClass != null ? $" class=\"{cssClass}\"" : "";
    _html.Append($"<div{classAttr}>{content}</div>");
    _expectedPlainText.Add(content);
    return this;
  }

  public HtmlContentBuilder WithSpan(string content, string? cssClass = null)
  {
    var classAttr = cssClass != null ? $" class=\"{cssClass}\"" : "";
    _html.Append($"<span{classAttr}>{content}</span>");
    _expectedPlainText.Add(content);
    return this;
  }

  public HtmlContentBuilder WithLineBreak()
  {
    _html.Append("<br>");
    _expectedPlainText.Add("\n");
    return this;
  }

  public HtmlContentBuilder WithLineBreakSelfClosing()
  {
    _html.Append("<br/>");
    _expectedPlainText.Add("\n");
    return this;
  }

  public HtmlContentBuilder WithListItem(string content)
  {
    _html.Append($"<li>{content}</li>");
    _expectedPlainText.Add($"• {content}");
    return this;
  }

  public HtmlContentBuilder WithUnorderedList(params string[] items)
  {
    _html.Append("<ul>");
    foreach (var item in items)
    {
      WithListItem(item);
    }
    _html.Append("</ul>");
    return this;
  }

  public HtmlContentBuilder WithOrderedList(params string[] items)
  {
    _html.Append("<ol>");
    foreach (var item in items)
    {
      WithListItem(item);
    }
    _html.Append("</ol>");
    return this;
  }

  public HtmlContentBuilder WithLink(string text, string href)
  {
    _html.Append($"<a href=\"{href}\">{text}</a>");
    _expectedPlainText.Add(text);
    return this;
  }

  public HtmlContentBuilder WithImage(string src, string alt)
  {
    _html.Append($"<img src=\"{src}\" alt=\"{alt}\"/>");
    // Images typically don't add text content in HtmlStripper
    return this;
  }

  public HtmlContentBuilder WithStrong(string content)
  {
    _html.Append($"<strong>{content}</strong>");
    _expectedPlainText.Add(content);
    return this;
  }

  public HtmlContentBuilder WithEmphasis(string content)
  {
    _html.Append($"<em>{content}</em>");
    _expectedPlainText.Add(content);
    return this;
  }

  public HtmlContentBuilder WithScript(string scriptContent)
  {
    _html.Append($"<script>{scriptContent}</script>");
    // Script content is completely removed by HtmlStripper
    return this;
  }

  public HtmlContentBuilder WithStyle(string styleContent)
  {
    _html.Append($"<style>{styleContent}</style>");
    // Style content is typically removed
    _expectedPlainText.Add(styleContent); // Depends on implementation
    return this;
  }

  public HtmlContentBuilder WithComment(string commentContent)
  {
    _html.Append($"<!-- {commentContent} -->");
    // Comments are typically removed
    return this;
  }

  public HtmlContentBuilder WithHtmlEntity(string entity)
  {
    _html.Append(entity);
    // Entity decoding depends on the specific entity
    var decoded = System.Net.WebUtility.HtmlDecode(entity);
    if (entity == "&nbsp;")
    {
      // HtmlStripper removes non-breaking spaces entirely
      // Don't add to expected text
    }
    else
    {
      _expectedPlainText.Add(decoded);
    }
    return this;
  }

  public HtmlContentBuilder WithRawText(string text)
  {
    _html.Append(text);
    _expectedPlainText.Add(text);
    return this;
  }

  public HtmlContentBuilder WithNonBreakingSpace()
  {
    _html.Append("&nbsp;");
    // HtmlStripper removes non-breaking spaces
    return this;
  }

  public HtmlContentBuilder WithNestedStructure(Action<HtmlContentBuilder> nestedContent)
  {
    var nestedBuilder = new HtmlContentBuilder();
    nestedContent(nestedBuilder);
    _html.Append(nestedBuilder.Build());
    _expectedPlainText.AddRange(nestedBuilder._expectedPlainText);
    return this;
  }

  public HtmlContentBuilder WithMalformedTag(string tagName, string content)
  {
    _html.Append($"<{tagName}>{content}");
    // Malformed tags might still have content extracted
    _expectedPlainText.Add(content);
    return this;
  }

  public HtmlContentBuilder WithMultipleNewlines(int count)
  {
    _html.Append(string.Concat(Enumerable.Repeat("\n", count)));
    // HtmlStripper collapses excessive newlines
    _expectedPlainText.Add(count > 2 ? "\n\n" : string.Concat(Enumerable.Repeat("\n", count)));
    return this;
  }

  public HtmlContentBuilder WithComplexTable(List<List<string>> rows)
  {
    _html.Append("<table>");
    foreach (var row in rows)
    {
      _html.Append("<tr>");
      foreach (var cell in row)
      {
        _html.Append($"<td>{cell}</td>");
        _expectedPlainText.Add(cell);
      }
      _html.Append("</tr>");
    }
    _html.Append("</table>");
    return this;
  }

  public HtmlContentBuilder WithWhitespace(string whitespace)
  {
    _html.Append(whitespace);
    // Leading/trailing whitespace is typically trimmed
    if (!string.IsNullOrWhiteSpace(whitespace))
    {
      _expectedPlainText.Add(whitespace.Trim());
    }
    return this;
  }

  /// <summary>
  /// Builds the final HTML string
  /// </summary>
  public string Build()
  {
    return _html.ToString();
  }

  /// <summary>
  /// Gets the expected plain text result after stripping HTML
  /// </summary>
  public string GetExpectedPlainText()
  {
    var result = string.Join("", _expectedPlainText);
    // Apply HtmlStripper's final transformations
    result = System.Text.RegularExpressions.Regex.Replace(result, @"\n{3,}", "\n\n");
    return result.Trim();
  }

  /// <summary>
  /// Creates a builder with a standard HTML document structure
  /// </summary>
  public static HtmlContentBuilder CreateDocument()
  {
    return new HtmlContentBuilder()
        .WithRawText("<!DOCTYPE html>")
        .WithRawText("<html>")
        .WithRawText("<head><title>Test Document</title></head>")
        .WithRawText("<body>");
  }

  /// <summary>
  /// Closes a standard HTML document
  /// </summary>
  public HtmlContentBuilder CloseDocument()
  {
    return WithRawText("</body></html>");
  }

  /// <summary>
  /// Creates various edge case HTML samples for testing
  /// </summary>
  public static class EdgeCases
  {
    public static HtmlContentBuilder EmptyTags()
    {
      return new HtmlContentBuilder()
          .WithRawText("<div></div>")
          .WithRawText("<p></p>")
          .WithRawText("<span></span>");
    }

    public static HtmlContentBuilder NestedEmptyTags()
    {
      return new HtmlContentBuilder()
          .WithRawText("<div><p><span></span></p></div>");
    }

    public static HtmlContentBuilder ScriptAndStyleHeavy()
    {
      return new HtmlContentBuilder()
          .WithScript("console.log('test');")
          .WithRawText("Content between scripts")
          .WithStyle("body { color: red; }")
          .WithRawText("More content");
    }

    public static HtmlContentBuilder MixedCaseTags()
    {
      return new HtmlContentBuilder()
          .WithRawText("<DIV>Upper case div</DIV>")
          .WithRawText("<Span>Mixed case span</Span>")
          .WithRawText("<SCRIPT>alert('test');</SCRIPT>");
    }

    public static HtmlContentBuilder ComplexNesting()
    {
      return new HtmlContentBuilder()
          .WithDiv("Outer")
          .WithNestedStructure(nested => nested
              .WithDiv("Middle")
              .WithNestedStructure(inner => inner
                  .WithSpan("Inner")
                  .WithStrong("Deep")));
    }

    public static HtmlContentBuilder SpecialCharactersInAttributes()
    {
      return new HtmlContentBuilder()
          .WithRawText("<div data-value='test\"value'>Content</div>")
          .WithRawText("<a href=\"javascript:alert('xss')\">Link</a>");
    }
  }
}