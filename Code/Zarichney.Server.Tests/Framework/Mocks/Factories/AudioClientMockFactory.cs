using Microsoft.Extensions.Logging;
using Moq;
using Zarichney.Services.AI;
using Zarichney.Services.Email;

namespace Zarichney.Tests.Framework.Mocks.Factories;

/// <summary>
/// Factory for creating mock dependencies for TranscribeService testing.
/// </summary>
public static class AudioClientMockFactory
{
    public static Mock<IEmailService> CreateEmailServiceMock()
    {
        var mock = new Mock<IEmailService>();
        
        mock.Setup(x => x.SendErrorNotification(
                It.IsAny<string>(), 
                It.IsAny<Exception>(), 
                It.IsAny<string>(), 
                It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);
            
        return mock;
    }

    public static Mock<ILogger<TranscribeService>> CreateLoggerMock()
    {
        return new Mock<ILogger<TranscribeService>>();
    }
}