namespace Keepi.Generators.Unit.Tests;

public class TestContextTargetDependencyTests
{
    [Fact]
    public void Constructor_returns_expected_result_for_interface_without_methods()
    {
        var result = new TestContextTargetDependency(
            fullName: "Keepi.Core.IMyInterface",
            shortName: "MyInterface",
            methods: [],
            verifyLogging: false
        );

        result.FullName.ShouldBe("Keepi.Core.IMyInterface");
        result.ShortName.ShouldBe("MyInterface");
        result.MockName.ShouldBe("MyInterfaceMock");
        result.IsLooseMock.ShouldBeFalse();
        result.Methods.ShouldBeEmpty();
        result.IsVerified.ShouldBeTrue();
        result.GenerateWithCallMethods.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_returns_expected_result_for_interface_with_methods()
    {
        var result = new TestContextTargetDependency(
            fullName: "Keepi.Core.IMyInterface",
            shortName: "MyInterface",
            methods:
            [
                new(
                    name: "Execute",
                    parameterTypeFullNames: ["System.Threading.CancellationToken"],
                    returnTypeFullName: "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>",
                    useAsyncReturn: true
                ),
            ],
            verifyLogging: false
        );

        result.FullName.ShouldBe("Keepi.Core.IMyInterface");
        result.ShortName.ShouldBe("MyInterface");
        result.MockName.ShouldBe("MyInterfaceMock");
        result.IsLooseMock.ShouldBeFalse();

        result.Methods.ShouldHaveSingleItem();
        result.Methods[0].Name.ShouldBe("Execute");
        result
            .Methods[0]
            .ParameterTypeFullNames.ShouldBeEquivalentTo(
                new string[] { "System.Threading.CancellationToken" }
            );
        result
            .Methods[0]
            .ReturnTypeFullName.ShouldBe(
                "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>"
            );
        result.Methods[0].UseAsyncReturn.ShouldBeTrue();

        result.IsVerified.ShouldBeTrue();
        result.GenerateWithCallMethods.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_returns_loose_mock_for_microsoft_logger()
    {
        var result = new TestContextTargetDependency(
            fullName: "Microsoft.Extensions.Logging.ILogger<Keepi.Core.IMyInterface>",
            shortName: "Logger",
            methods: [],
            verifyLogging: false
        );

        result.FullName.ShouldBe("Microsoft.Extensions.Logging.ILogger<Keepi.Core.IMyInterface>");
        result.ShortName.ShouldBe("Logger");
        result.MockName.ShouldBe("LoggerMock");
        result.IsLooseMock.ShouldBeTrue();
        result.Methods.ShouldBeEmpty();
        result.IsVerified.ShouldBeFalse();
        result.GenerateWithCallMethods.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_returns_verified_mock_for_microsoft_logger()
    {
        var result = new TestContextTargetDependency(
            fullName: "Microsoft.Extensions.Logging.ILogger<Keepi.Core.IMyInterface>",
            shortName: "Logger",
            methods: [],
            verifyLogging: true
        );

        result.FullName.ShouldBe("Microsoft.Extensions.Logging.ILogger<Keepi.Core.IMyInterface>");
        result.ShortName.ShouldBe("Logger");
        result.MockName.ShouldBe("LoggerMock");
        result.IsLooseMock.ShouldBeTrue();
        result.Methods.ShouldBeEmpty();
        result.IsVerified.ShouldBeTrue();
        result.GenerateWithCallMethods.ShouldBeFalse();
    }
}
