using Microsoft.Extensions.Logging;

namespace Keepi.Core.Unit.Tests;

internal static class MoqExtensions
{
    public static void VerifyWarningLog<TLogTarget>(
        this Mock<ILogger<TLogTarget>> mock,
        string expectedMessage
    )
    {
        VerifyLog(mock: mock, expectedLogLevel: LogLevel.Warning, expectedMessage: expectedMessage);
    }

    public static void VerifyLog<TLogTarget>(
        this Mock<ILogger<TLogTarget>> mock,
        LogLevel expectedLogLevel,
        string expectedMessage
    )
    {
        mock.Verify(logger =>
            logger.Log(
                expectedLogLevel,
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>(
                    (@object, @type) =>
                        @object.ToString() == expectedMessage && @type.Name == "FormattedLogValues"
                ),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );
    }
}
