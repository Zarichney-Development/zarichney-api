using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OpenAI.Chat;
using Zarichney.Server.Services.Emails;
using Zarichney.Server.Services.GitHub;
using Zarichney.Server.Services.Sessions;

namespace Zarichney.Server.Services.AI;

public class LlmConversation
{
  public string Id { get; init; } = string.Empty;
  public string SystemPrompt { get; init; } = string.Empty;
  public string? PromptCatalogName { get; init; } = string.Empty;
  public List<LlmMessage> Messages { get; } = [];
}

public class LlmMessage
{
  public ChatCompletionOptions? Options { get; set; }
  public string Request { get; init; } = string.Empty;
  public required object Response { get; init; }
  public DateTime Timestamp { get; init; }
  public required ChatCompletion ChatCompletion { get; init; }
}

public interface ILlmRepository
{
  Task WriteConversationAsync(LlmConversation conversation, Session session);
}

public class LlmRepository(IGitHubService githubService) : ILlmRepository
{
  private static readonly JsonSerializerOptions IndentedOptions = new()
  {
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  private static string IndentAllLines(string text, int indentLevel)
  {
    var indent = new string(' ', indentLevel);
    return string.Join("\n" + indent, text.Split('\n'));
  }

  private static string FormatEmbeddedJson(string jsonContent)
  {
    try
    {
      using var doc = JsonDocument.Parse(jsonContent);
      return JsonSerializer.Serialize(doc.RootElement, IndentedOptions);
    }
    catch
    {
      return jsonContent;
    }
  }

  private static string FormatRequestContent(string request)
  {
    var lines = new List<string>();
    var parts = request.Split(new[] { "```json", "```" }, StringSplitOptions.None);

    for (var i = 0; i < parts.Length; i++)
    {
      if (i % 2 == 0)
      {
        // Non-JSON content
        lines.AddRange(parts[i].Split('\n', StringSplitOptions.RemoveEmptyEntries));
      }
      else
      {
        // JSON content within code blocks
        lines.Add("```json");
        var formattedJson = FormatEmbeddedJson(parts[i].Trim());
        lines.AddRange(formattedJson.Split('\n'));
        lines.Add("```");
      }
    }

    return string.Join("\n", lines);
  }

  private static JsonNode SerializeResponse(object response)
  {
    if (response is string strResponse)
    {
      try
      {
        using var doc = JsonDocument.Parse(strResponse);
        return JsonNode.Parse(strResponse)!;
      }
      catch
      {
        return JsonValue.Create(strResponse);
      }
    }

    var responseJson = JsonSerializer.Serialize(response, IndentedOptions);
    return JsonNode.Parse(responseJson)!;
  }

  private class MultilineStringConverter(int indentLevel = 2) : JsonConverter<string>
  {
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
      if (value.Contains('\n'))
      {
        var formattedValue = IndentAllLines(value, indentLevel);
        writer.WriteStringValue("\n" + formattedValue);
      }
      else
      {
        writer.WriteStringValue(value);
      }
    }
  }

  private JsonObject CreateMessageJson(LlmMessage message)
  {
    var formattedRequest = FormatRequestContent(message.Request);

    return new JsonObject
    {
      ["options"] = message.Options != null
        ? JsonNode.Parse(JsonSerializer.Serialize(message.Options, IndentedOptions))
        : null,
      ["request"] = JsonValue.Create(formattedRequest),
      ["response"] = SerializeResponse(message.Response),
      ["timestamp"] = JsonValue.Create(message.Timestamp.ToString("o")),
      ["chatCompletion"] = JsonNode.Parse(JsonSerializer.Serialize(
        message.ChatCompletion,
        IndentedOptions
      ))
    };
  }

  public async Task WriteConversationAsync(LlmConversation conversation, Session session)
  {
    var baseDir = $"prompts/session_{session.CreatedAt:yyyyMMdd-HHmmss}_";

    var userEmail = session.Order?.Customer.Email;

    if (!string.IsNullOrEmpty(userEmail))
    {
      baseDir += $"{EmailService.MakeSafeFileName(userEmail)}";
    }
    else
    {
      baseDir += $"{session.Id.ToString()[..8]}";
    }

    if (!string.IsNullOrEmpty(conversation.PromptCatalogName))
    {
      baseDir += $"/{conversation.PromptCatalogName}";
    }

    baseDir += $"/{conversation.Id}";

    var serializerOptions = new JsonSerializerOptions(IndentedOptions);
    serializerOptions.Converters.Add(new MultilineStringConverter());

    for (var i = 0; i < conversation.Messages.Count; i++)
    {
      var fileName = $"message_{i + 1}.json";
      var message = conversation.Messages[i];

      var jsonObject = CreateMessageJson(message);
      var json = jsonObject.ToJsonString(serializerOptions);
      var bytes = Encoding.UTF8.GetBytes(json);

      await githubService.EnqueueCommitAsync(
        fileName,
        bytes,
        baseDir,
        $"Add {fileName} to conversation {conversation.Id}"
      );
    }

    const string convMetaFilename = "conversation.metadata.json";
    var convMetaJson = JsonSerializer.Serialize(
      new
      {
        conversation.Id,
        conversation.SystemPrompt,
        conversation.PromptCatalogName
      },
      serializerOptions
    );
    var convMetaBytes = Encoding.UTF8.GetBytes(convMetaJson);

    await githubService.EnqueueCommitAsync(
      convMetaFilename,
      convMetaBytes,
      baseDir,
      $"Add metadata for conversation {conversation.Id}"
    );
  }
}