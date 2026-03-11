namespace Keepi.Generators.Unit.Tests;

public class TestContextTargetDependencyTests
{
    [Fact]
    public void Constructor_returns_expected_result_for_interface()
    {
        var result = new TestContextTargetDependency(
            fullName: "Keepi.Core.IMyInterface",
            shortName: "MyInterface"
        );

        result.FullName.ShouldBe("Keepi.Core.IMyInterface");
        result.MockName.ShouldBe("MyInterfaceMock");
        result.IsLooseMock.ShouldBeFalse();
        result.IsVerified.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_returns_loose_mock_for_microsoft_logger()
    {
        var result = new TestContextTargetDependency(
            fullName: "Microsoft.Extensions.Logging.ILogger<Keepi.Core.IMyInterface>",
            shortName: "Logger"
        );

        result.FullName.ShouldBe("Microsoft.Extensions.Logging.ILogger<Keepi.Core.IMyInterface>");
        result.MockName.ShouldBe("LoggerMock");
        result.IsLooseMock.ShouldBeTrue();
        result.IsVerified.ShouldBeFalse();
    }
}
