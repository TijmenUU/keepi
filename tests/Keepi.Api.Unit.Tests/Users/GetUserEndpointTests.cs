using Keepi.Api.Users.Get;
using Keepi.Core;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Api.Unit.Tests.Users;

public class GetUserEndpointTests
{
    [Fact]
    public async Task HandleAsync_returns_expected_user()
    {
        var context = new GetUserEndpointTestContext().WithGetUserUseCaseCall(
            Result.Success<GetUserUseCaseOutput, GetUserUseCaseError>(
                new(
                    Id: 42,
                    Name: "John Doe",
                    EmailAddress: "johndoe@example.com",
                    EntriesPermission: UserPermission.ReadAndModify,
                    ExportsPermission: UserPermission.ReadAndModify,
                    ProjectsPermission: UserPermission.ReadAndModify,
                    UsersPermission: UserPermission.ReadAndModify
                )
            )
        );

        var endpoint = context.BuildTarget();

        await endpoint.HandleAsync(cancellationToken: CancellationToken.None);
        endpoint.Response.ShouldBeEquivalentTo(
            new GetUserResponse(
                Id: 42,
                Name: "John Doe",
                EmailAddress: "johndoe@example.com",
                EntriesPermission: GetUserResponsePermission.ReadAndModify,
                ExportsPermission: GetUserResponsePermission.ReadAndModify,
                ProjectsPermission: GetUserResponsePermission.ReadAndModify,
                UsersPermission: GetUserResponsePermission.ReadAndModify
            )
        );

        context.GetUserUseCaseMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [MemberData(nameof(PermissionMappingTestCases))]
    public async Task HandleAsync_maps_EntriesPermission_correctly(
        UserPermission permissionInput,
        GetUserResponsePermission expectedPermission
    )
    {
        var context = new GetUserEndpointTestContext().WithGetUserUseCaseCall(
            Result.Success<GetUserUseCaseOutput, GetUserUseCaseError>(
                new(
                    Id: 42,
                    Name: "John Doe",
                    EmailAddress: "johndoe@example.com",
                    EntriesPermission: permissionInput,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: UserPermission.None
                )
            )
        );

        var endpoint = context.BuildTarget();

        await endpoint.HandleAsync(cancellationToken: CancellationToken.None);
        endpoint.Response.EntriesPermission.ShouldBe(expectedPermission);
    }

    [Theory]
    [MemberData(nameof(PermissionMappingTestCases))]
    public async Task HandleAsync_maps_ExportsPermission_correctly(
        UserPermission permissionInput,
        GetUserResponsePermission expectedPermission
    )
    {
        var context = new GetUserEndpointTestContext().WithGetUserUseCaseCall(
            Result.Success<GetUserUseCaseOutput, GetUserUseCaseError>(
                new(
                    Id: 42,
                    Name: "John Doe",
                    EmailAddress: "johndoe@example.com",
                    EntriesPermission: UserPermission.None,
                    ExportsPermission: permissionInput,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: UserPermission.None
                )
            )
        );

        var endpoint = context.BuildTarget();

        await endpoint.HandleAsync(cancellationToken: CancellationToken.None);
        endpoint.Response.ExportsPermission.ShouldBe(expectedPermission);
    }

    [Theory]
    [MemberData(nameof(PermissionMappingTestCases))]
    public async Task HandleAsync_maps_ProjectsPermission_correctly(
        UserPermission permissionInput,
        GetUserResponsePermission expectedPermission
    )
    {
        var context = new GetUserEndpointTestContext().WithGetUserUseCaseCall(
            Result.Success<GetUserUseCaseOutput, GetUserUseCaseError>(
                new(
                    Id: 42,
                    Name: "John Doe",
                    EmailAddress: "johndoe@example.com",
                    EntriesPermission: UserPermission.None,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: permissionInput,
                    UsersPermission: UserPermission.None
                )
            )
        );

        var endpoint = context.BuildTarget();

        await endpoint.HandleAsync(cancellationToken: CancellationToken.None);
        endpoint.Response.ProjectsPermission.ShouldBe(expectedPermission);
    }

    [Theory]
    [MemberData(nameof(PermissionMappingTestCases))]
    public async Task HandleAsync_maps_UsersPermission_correctly(
        UserPermission permissionInput,
        GetUserResponsePermission expectedPermission
    )
    {
        var context = new GetUserEndpointTestContext().WithGetUserUseCaseCall(
            Result.Success<GetUserUseCaseOutput, GetUserUseCaseError>(
                new(
                    Id: 42,
                    Name: "John Doe",
                    EmailAddress: "johndoe@example.com",
                    EntriesPermission: UserPermission.None,
                    ExportsPermission: UserPermission.None,
                    ProjectsPermission: UserPermission.None,
                    UsersPermission: permissionInput
                )
            )
        );

        var endpoint = context.BuildTarget();

        await endpoint.HandleAsync(cancellationToken: CancellationToken.None);
        endpoint.Response.UsersPermission.ShouldBe(expectedPermission);
    }

    public static TheoryData<UserPermission, GetUserResponsePermission> PermissionMappingTestCases()
    {
        return new()
        {
            { UserPermission.None, GetUserResponsePermission.None },
            { UserPermission.Read, GetUserResponsePermission.Read },
            { UserPermission.ReadAndModify, GetUserResponsePermission.ReadAndModify },
        };
    }

    [Fact]
    public async Task HandleAsync_returns_401_for_unauthenticated_user()
    {
        var context = new GetUserEndpointTestContext().WithGetUserUseCaseCall(
            Result.Failure<GetUserUseCaseOutput, GetUserUseCaseError>(
                GetUserUseCaseError.UnauthenticatedUser
            )
        );

        var endpoint = context.BuildTarget();

        await endpoint.HandleAsync(cancellationToken: CancellationToken.None);
        endpoint.HttpContext.Response.StatusCode.ShouldBe(expected: 401);
    }

    [Fact]
    public async Task HandleAsync_returns_500_for_unknown_error()
    {
        var context = new GetUserEndpointTestContext().WithGetUserUseCaseCall(
            Result.Failure<GetUserUseCaseOutput, GetUserUseCaseError>(GetUserUseCaseError.Unknown)
        );

        var endpoint = context.BuildTarget();

        await endpoint.HandleAsync(cancellationToken: CancellationToken.None);
        endpoint.HttpContext.Response.StatusCode.ShouldBe(expected: 500);
    }
}

[GenerateTestContext(TargetType = typeof(GetUserEndpoint), GenerateWithCallMethods = true)]
internal partial class GetUserEndpointTestContext { }
