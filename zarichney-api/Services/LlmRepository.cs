using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using OpenAI.Chat;
using Zarichney.Services.Sessions;

namespace Zarichney.Services;

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
  public string Response { get; init; } = string.Empty;
  public DateTime Timestamp { get; set; }
  public required ChatCompletion ChatCompletion { get; set; }
}

public interface ILlmRepository
{
  Task WriteConversationAsync(LlmConversation conversation, Session session);
}

public class LlmRepository(
  IGitHubService githubService
) : ILlmRepository
{
  public async Task WriteConversationAsync(LlmConversation conversation, Session session)
  {
    var baseDir = $"prompts/session_{session.CreatedAt:yyyyMMdd-HHmmss}_{session.Id.ToString()[..8]}";
    if (!string.IsNullOrEmpty(conversation.PromptCatalogName))
    {
      baseDir += $"/{conversation.PromptCatalogName}";
    }

    baseDir += $"/{conversation.Id}";

    var serializerOptions = new JsonSerializerOptions
    {
      WriteIndented = true,
      Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    for (var i = 0; i < conversation.Messages.Count; i++)
    {
      var fileName = $"message_{i + 1}.json";
      var message = conversation.Messages[i];
      var json = JsonSerializer.Serialize(message, serializerOptions);
      var bytes = Encoding.UTF8.GetBytes(json);

      await githubService.EnqueueCommitAsync(fileName, bytes, baseDir,
        $"Add {fileName} to conversation {conversation.Id}");
    }

    const string convMetaFilename = "conversation.json";
    var convMetaJson = JsonSerializer.Serialize(new
    {
      conversation.Id,
      conversation.SystemPrompt,
      conversation.PromptCatalogName
    }, serializerOptions);
    var convMetaBytes = Encoding.UTF8.GetBytes(convMetaJson);

    await githubService.EnqueueCommitAsync(convMetaFilename, convMetaBytes, baseDir,
      $"Add metadata for conversation {conversation.Id}");
  }
}