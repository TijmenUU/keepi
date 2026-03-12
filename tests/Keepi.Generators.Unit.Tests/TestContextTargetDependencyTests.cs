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
    public void Constructor_returns_expected_result_for_interface_with_method()
    {
        var result = new TestContextTargetDependency(
            fullName: "Keepi.Core.IMyInterface",
            shortName: "MyInterface",
            methods:
            [
                new TestContextTargetDependencyMethod(
                    name: "Execute",
                    parameterTypeFullNames: ["System.Threading.CancellationToken"],
                    returnTypeFullName: "int",
                    useAsyncReturn: false
                ),
            ],
            verifyLogging: false
        );

        result.FullName.ShouldBe("Keepi.Core.IMyInterface");
        result.ShortName.ShouldBe("MyInterface");
        result.MockName.ShouldBe("MyInterfaceMock");
        result.IsLooseMock.ShouldBeFalse();

        result.Methods.ShouldHaveSingleItem();
        var resultMethod = result.Methods[0].ShouldBeOfType<TestContextTargetDependencyMethod>();
        resultMethod.Name.ShouldBe("Execute");
        resultMethod.ParameterTypeFullNames.ShouldBeEquivalentTo(
            new string[] { "System.Threading.CancellationToken" }
        );
        resultMethod.ReturnTypeFullName.ShouldBe("int");
        resultMethod.Kind.ShouldBe(TestContextTargetDependencyMethodKind.Method);

        result.IsVerified.ShouldBeTrue();
        result.GenerateWithCallMethods.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_returns_expected_result_for_interface_with_async_method()
    {
        var result = new TestContextTargetDependency(
            fullName: "Keepi.Core.IMyInterface",
            shortName: "MyInterface",
            methods:
            [
                new TestContextTargetDependencyMethod(
                    name: "Execute",
                    parameterTypeFullNames: ["System.Threading.CancellationToken"],
                    returnTypeFullName: "int",
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
        var resultMethod = result.Methods[0].ShouldBeOfType<TestContextTargetDependencyMethod>();
        resultMethod.Name.ShouldBe("Execute");
        resultMethod.ParameterTypeFullNames.ShouldBeEquivalentTo(
            new string[] { "System.Threading.CancellationToken" }
        );
        resultMethod.ReturnTypeFullName.ShouldBe("int");
        resultMethod.Kind.ShouldBe(TestContextTargetDependencyMethodKind.AsyncMethod);

        result.IsVerified.ShouldBeTrue();
        result.GenerateWithCallMethods.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_returns_expected_result_for_interface_with_result_method()
    {
        var result = new TestContextTargetDependency(
            fullName: "Keepi.Core.IMyInterface",
            shortName: "MyInterface",
            methods:
            [
                new TestContextTargetDependencyResultMethod(
                    name: "Execute",
                    parameterTypeFullNames: ["System.Threading.CancellationToken"],
                    returnTypeFullName: "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>",
                    resultErrorTypeFullName: "Keepi.Core.Users.GetUserUseCaseError",
                    resultSuccessTypeFullName: "Keepi.Core.Users.GetUserUseCaseOutput",
                    useAsyncReturn: false
                ),
            ],
            verifyLogging: false
        );

        result.FullName.ShouldBe("Keepi.Core.IMyInterface");
        result.ShortName.ShouldBe("MyInterface");
        result.MockName.ShouldBe("MyInterfaceMock");
        result.IsLooseMock.ShouldBeFalse();

        result.Methods.ShouldHaveSingleItem();
        var resultMethod = result
            .Methods[0]
            .ShouldBeOfType<TestContextTargetDependencyResultMethod>();
        resultMethod.Name.ShouldBe("Execute");
        resultMethod.ParameterTypeFullNames.ShouldBeEquivalentTo(
            new string[] { "System.Threading.CancellationToken" }
        );
        resultMethod.ReturnTypeFullName.ShouldBe(
            "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>"
        );
        resultMethod.ResultSuccessTypeFullName.ShouldBe("Keepi.Core.Users.GetUserUseCaseOutput");
        resultMethod.ResultErrorTypeFullName.ShouldBe("Keepi.Core.Users.GetUserUseCaseError");
        resultMethod.Kind.ShouldBe(TestContextTargetDependencyMethodKind.ResultMethod);

        result.IsVerified.ShouldBeTrue();
        result.GenerateWithCallMethods.ShouldBeTrue();
    }

    [Fact]
    public void Constructor_returns_expected_result_for_interface_with_async_result_method()
    {
        var result = new TestContextTargetDependency(
            fullName: "Keepi.Core.IMyInterface",
            shortName: "MyInterface",
            methods:
            [
                new TestContextTargetDependencyResultMethod(
                    name: "Execute",
                    parameterTypeFullNames: ["System.Threading.CancellationToken"],
                    returnTypeFullName: "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>",
                    resultErrorTypeFullName: "Keepi.Core.Users.GetUserUseCaseError",
                    resultSuccessTypeFullName: "Keepi.Core.Users.GetUserUseCaseOutput",
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
        var resultMethod = result
            .Methods[0]
            .ShouldBeOfType<TestContextTargetDependencyResultMethod>();
        resultMethod.Name.ShouldBe("Execute");
        resultMethod.ParameterTypeFullNames.ShouldBeEquivalentTo(
            new string[] { "System.Threading.CancellationToken" }
        );
        resultMethod.ReturnTypeFullName.ShouldBe(
            "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>"
        );
        resultMethod.ResultSuccessTypeFullName.ShouldBe("Keepi.Core.Users.GetUserUseCaseOutput");
        resultMethod.ResultErrorTypeFullName.ShouldBe("Keepi.Core.Users.GetUserUseCaseError");
        resultMethod.Kind.ShouldBe(TestContextTargetDependencyMethodKind.AsyncResultMethod);

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
