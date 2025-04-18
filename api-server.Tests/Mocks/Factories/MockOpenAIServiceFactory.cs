using Moq;
using Zarichney.Services.AI;

namespace Zarichney.Tests.Mocks.Factories;

/// <summary>
/// Factory for creating mock OpenAI service (ILlmService).
/// </summary>
public class MockOpenAIServiceFactory : BaseMockFactory<ILlmService>
{
    /// <summary>
    /// Creates a mock OpenAI service with default implementations.
    /// </summary>
    /// <returns>A Mock of the ILlmService.</returns>
    public new static Mock<ILlmService> CreateMock()
    {
        var factory = new MockOpenAIServiceFactory();
        return factory.CreateDefaultMock();
    }

    /// <summary>
    /// Sets up default behaviors for the mock OpenAI service.
    /// </summary>
    /// <param name="mock">The mock to set up.</param>
    protected override void SetupDefaultMock(Mock<ILlmService> mock)
    {
        // Optionally, set up default behaviors for real ILlmService methods here.
        // For example, you could set up GetCompletionContent to return a default value:
        // mock.Setup(s => s.GetCompletionContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OpenAI.Chat.ChatCompletionOptions>(), It.IsAny<int?>()))
        //     .ReturnsAsync("default response");
    }
}
