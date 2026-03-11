namespace Keepi.Generators.Unit.Tests;

public class UnitTest1
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
    public void Create_returns_expected_test_context_content_with_generated_with_result_methods()
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
                        new(
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
    public GetUserEndpointTestContext WithGetUserUseCaseCall(Keepi.Core.IValueOrErrorResult<Keepi.Core.Users.GetUserUseCaseOutput, Keepi.Core.Users.GetUserUseCaseError> result)
    {
        GetUserUseCaseMock.Setup(x => x.Execute(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(result);
        return this;
    }
}
"
        );
    }
}
