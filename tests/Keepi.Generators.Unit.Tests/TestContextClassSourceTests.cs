namespace Keepi.Generators.Unit.Tests;

public class TestContextClassSourceTests
{
    [Fact]
    public void Create_returns_expected_test_context_content_for_use_case()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Core.Unit.Tests.Exports",
            className: "ExportUserEntriesUseCaseTestContext",
            targetFullName: "Keepi.Core.Exports.ExportUserEntriesUseCase",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IResolveUser",
                    shortName: "ResolveUser",
                    methods: [],
                    verifyLogging: false
                ),
                new(
                    fullName: "Keepi.Core.Exports.IGetExportUserEntries",
                    shortName: "GetExportUserEntries",
                    methods: [],
                    verifyLogging: false
                ),
            ],
            targetIsFastEndpoint: false
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Core.Unit.Tests.Exports;
internal partial class ExportUserEntriesUseCaseTestContext
{
    public Mock<Keepi.Core.Users.IResolveUser> ResolveUserMock { get; } = new(MockBehavior.Strict);
    public Mock<Keepi.Core.Exports.IGetExportUserEntries> GetExportUserEntriesMock { get; } = new(MockBehavior.Strict);
    public Keepi.Core.Exports.ExportUserEntriesUseCase BuildTarget()
    {
        return new Keepi.Core.Exports.ExportUserEntriesUseCase(ResolveUserMock.Object, GetExportUserEntriesMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        ResolveUserMock.VerifyNoOtherCalls();
        GetExportUserEntriesMock.VerifyNoOtherCalls();
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_for_fast_endpoint()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
                    methods: [],
                    verifyLogging: false
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_with_generated_with_method()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
                    methods:
                    [
                        new TestContextTargetDependencyMethod(
                            name: "Execute",
                            parameterTypeFullNames: ["System.Threading.CancellationToken"],
                            returnTypeFullName: "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>",
                            useAsyncReturn: false
                        ),
                    ],
                    verifyLogging: false
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
    public GetUserEndpointTestContext WithGetUserUseCaseCall(Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError> returnValue)
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>())).Returns(returnValue);
        return this;
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_with_generated_with_async_method()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
                    methods:
                    [
                        new TestContextTargetDependencyMethod(
                            name: "Execute",
                            parameterTypeFullNames: ["System.Threading.CancellationToken"],
                            returnTypeFullName: "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>",
                            useAsyncReturn: true
                        ),
                    ],
                    verifyLogging: false
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
    public GetUserEndpointTestContext WithGetUserUseCaseCall(Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError> returnValue)
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(returnValue);
        return this;
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_with_generated_with_parameterless_method()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
                    methods:
                    [
                        new TestContextTargetDependencyMethod(
                            name: "Execute",
                            parameterTypeFullNames: [],
                            returnTypeFullName: "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>",
                            useAsyncReturn: true
                        ),
                    ],
                    verifyLogging: false
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
    public GetUserEndpointTestContext WithGetUserUseCaseCall(Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError> returnValue)
    {
        GetUserUseCaseMock.Setup(x => x.Execute()).ReturnsAsync(returnValue);
        return this;
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_with_generated_with_void_method()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
                    methods:
                    [
                        new TestContextTargetDependencyMethod(
                            name: "Execute",
                            parameterTypeFullNames: ["System.Threading.CancellationToken"],
                            returnTypeFullName: "void",
                            useAsyncReturn: false
                        ),
                    ],
                    verifyLogging: false
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
    public GetUserEndpointTestContext WithGetUserUseCaseCall()
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>()));
        return this;
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_with_generated_with_task_method()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
                    methods:
                    [
                        new TestContextTargetDependencyMethod(
                            name: "Execute",
                            parameterTypeFullNames: ["System.Threading.CancellationToken"],
                            returnTypeFullName: "System.Threading.Tasks.Task",
                            useAsyncReturn: true
                        ),
                    ],
                    verifyLogging: false
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
    public GetUserEndpointTestContext WithGetUserUseCaseCall()
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>())).Returns(Task.CompletedTask);
        return this;
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_with_generated_with_multiple_methods()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
                    methods:
                    [
                        new TestContextTargetDependencyMethod(
                            name: "Execute",
                            parameterTypeFullNames: ["System.Threading.CancellationToken"],
                            returnTypeFullName: "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>",
                            useAsyncReturn: true
                        ),
                        new TestContextTargetDependencyMethod(
                            name: "Execute2",
                            parameterTypeFullNames: ["int"],
                            returnTypeFullName: "double",
                            useAsyncReturn: true
                        ),
                    ],
                    verifyLogging: false
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
    public GetUserEndpointTestContext WithGetUserUseCaseExecuteCall(Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError> returnValue)
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(returnValue);
        return this;
    }
    public GetUserEndpointTestContext WithGetUserUseCaseExecute2Call(double returnValue)
    {
        GetUserUseCaseMock.Setup(x => x.Execute2(It.IsAny<int>())).ReturnsAsync(returnValue);
        return this;
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_with_generated_with_result_method()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
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
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
    public GetUserEndpointTestContext WithGetUserUseCaseSuccess(Keepi.Core.Users.GetUserUseCaseOutput result)
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>())).Returns(Keepi.Core.Result.Success<Keepi.Core.Users.GetUserUseCaseOutput,Keepi.Core.Users.GetUserUseCaseError>(result));
        return this;
    }
    public GetUserEndpointTestContext WithGetUserUseCaseError(Keepi.Core.Users.GetUserUseCaseError error)
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>())).Returns(Keepi.Core.Result.Failure<Keepi.Core.Users.GetUserUseCaseOutput,Keepi.Core.Users.GetUserUseCaseError>(error));
        return this;
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_with_generated_with_async_result_method()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
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
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
    public GetUserEndpointTestContext WithGetUserUseCaseSuccess(Keepi.Core.Users.GetUserUseCaseOutput result)
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(Keepi.Core.Result.Success<Keepi.Core.Users.GetUserUseCaseOutput,Keepi.Core.Users.GetUserUseCaseError>(result));
        return this;
    }
    public GetUserEndpointTestContext WithGetUserUseCaseError(Keepi.Core.Users.GetUserUseCaseError error)
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(Keepi.Core.Result.Failure<Keepi.Core.Users.GetUserUseCaseOutput,Keepi.Core.Users.GetUserUseCaseError>(error));
        return this;
    }
}
"
        );
    }

    [Fact]
    public void Create_returns_expected_test_context_content_with_generated_with_property()
    {
        var result = TestContextClassSource.Create(
            @namespace: "Keepi.Api.Unit.Tests.Users",
            className: "GetUserEndpointTestContext",
            targetFullName: "Keepi.Api.Users.Get.GetUserEndpoint",
            targetDependencies:
            [
                new(
                    fullName: "Keepi.Core.Users.IGetUserUseCase",
                    shortName: "GetUserUseCase",
                    methods:
                    [
                        new TestContextTargetDependencyGetter(
                            name: "Execute",
                            returnTypeFullName: "Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError>"
                        ),
                    ],
                    verifyLogging: false
                ),
            ],
            targetIsFastEndpoint: true
        );

        result.Content.ShouldBe(
            @"using Moq;
using System.Threading.Tasks;
namespace Keepi.Api.Unit.Tests.Users;
internal partial class GetUserEndpointTestContext
{
    public Mock<Keepi.Core.Users.IGetUserUseCase> GetUserUseCaseMock { get; } = new(MockBehavior.Strict);
    public Keepi.Api.Users.Get.GetUserEndpoint BuildTarget()
    {
        return FastEndpoints.Factory.Create<Keepi.Api.Users.Get.GetUserEndpoint>(GetUserUseCaseMock.Object);
    }
    public void VerifyNoOtherCalls()
    {
        GetUserUseCaseMock.VerifyNoOtherCalls();
    }
    public GetUserEndpointTestContext WithGetUserUseCaseExecuteGet(Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError> result)
    {
        GetUserUseCaseMock.Setup(x => x.Execute).Returns(result);
        return this;
    }
}
"
        );
    }
}
