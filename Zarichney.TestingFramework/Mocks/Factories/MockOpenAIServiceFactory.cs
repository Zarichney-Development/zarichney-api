using Moq;
using Zarichney.Services.AI;
using OpenAI.Assistants;
using OpenAI.Chat;
using Zarichney.Cookbook.Prompts;

namespace Zarichney.TestingFramework.Mocks.Factories;

public class MockOpenAIServiceFactory : BaseMockFactory<ILlmService>
{
  public new static Mock<ILlmService> CreateMock()
  {
    var factory = new MockOpenAIServiceFactory();
    return factory.CreateDefaultMock();
  }

  protected override void SetupDefaultMock(Mock<ILlmService> mock)
  {
    // 1) Simple completion overload
    mock.Setup(s => s.GetCompletionContent(
        It.IsAny<string>(),
        It.IsAny<string?>(),
        It.IsAny<ChatCompletionOptions?>(),
        It.IsAny<int?>()))
      .ReturnsAsync("mock-response");

    // 2) Full LlmResult<string> overload
    mock.Setup(s => s.GetCompletionContent(
        It.IsAny<List<ChatMessage>>(),
        It.IsAny<string?>(),
        It.IsAny<ChatCompletionOptions?>(),
        It.IsAny<int?>()))
      .ReturnsAsync(new LlmResult<string>
      {
        Data = "mock-response",
        ConversationId = "mock-convo-id"
      });

    // 3) Assistant/thread/run lifecycle
    mock.Setup(s => s.CreateAssistant(It.IsAny<PromptBase>()))
      .ReturnsAsync("mock-assistant-id");
    mock.Setup(s => s.CreateThread())
      .ReturnsAsync("mock-thread-id");
    mock.Setup(s => s.CreateRun(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<bool>()))
      .ReturnsAsync("mock-run-id");

    // 4) Fire‐and‐forget methods
    mock.Setup(s => s.CreateMessage(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<MessageRole>()))
      .Returns(Task.CompletedTask);
    mock.Setup(s => s.SubmitToolOutputToRun(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<string>()))
      .Returns(Task.CompletedTask);
    mock.Setup(s => s.DeleteAssistant(It.IsAny<string>()))
      .Returns(Task.CompletedTask);
    mock.Setup(s => s.DeleteThread(It.IsAny<string>()))
      .Returns(Task.CompletedTask);

    // 5) Run status / cancellation
    mock.Setup(s => s.GetRun(
        It.IsAny<string>(),
        It.IsAny<string>()))
      .ReturnsAsync((false, RunStatus.Completed));
    mock.Setup(s => s.CancelRun(
        It.IsAny<string>(),
        It.IsAny<string>()))
      .ReturnsAsync("cancelled");

    mock.SetupCallFunction("RankRecipe", new RelevancyResult { Score = 100, Reasoning = "mocked relevant recipe" });
  }
}

public static class LlmServiceMockExtensions
{
  public static Mock<ILlmService> SetupCallFunction<T>(
    this Mock<ILlmService> mock,
    string functionName,
    T returnValue)
    where T : new()
  {
    mock.Setup(s => s.CallFunction<T>(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.Is<FunctionDefinition>(f => f.Name == functionName),
        It.IsAny<string?>(),
        It.IsAny<int?>()
      ))
      .ReturnsAsync((string _, string _, FunctionDefinition _, string? conv, int? _) =>
        new LlmResult<T>
        {
          Data = returnValue,
          ConversationId = conv ?? "mock-convo-id"
        });
    return mock;
  }
}
