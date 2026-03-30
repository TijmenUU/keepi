using Keepi.Core.Entries;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.Projects;

public class GetAllProjectsUseCaseTests
{
    [Fact]
    public async Task Execute_returns_projects()
    {
        var context = new GetAllProjectsUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetProjectsSuccess(
                new GetProjectsResultProject(
                    Id: ProjectId.From(1),
                    Name: ProjectName.From("Project 1"),
                    Enabled: true,
                    Users:
                    [
                        new(Id: UserId.From(10), Name: UserName.From("Henk")),
                        new(Id: UserId.From(11), Name: UserName.From("Piet")),
                    ],
                    InvoiceItems:
                    [
                        new(Id: InvoiceItemId.From(20), Name: InvoiceItemName.From("Post 1")),
                        new(Id: InvoiceItemId.From(21), Name: InvoiceItemName.From("Post 2")),
                    ]
                ),
                new GetProjectsResultProject(
                    Id: ProjectId.From(2),
                    Name: ProjectName.From("Project 2"),
                    Enabled: false,
                    Users: [],
                    InvoiceItems: []
                )
            );

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out var successResult, out _).ShouldBeTrue();

        successResult.ShouldBeEquivalentTo(
            new GetProjectsResult(
                Projects:
                [
                    new(
                        Id: ProjectId.From(1),
                        Name: ProjectName.From("Project 1"),
                        Enabled: true,
                        Users:
                        [
                            new(Id: UserId.From(10), Name: UserName.From("Henk")),
                            new(Id: UserId.From(11), Name: UserName.From("Piet")),
                        ],
                        InvoiceItems:
                        [
                            new(Id: InvoiceItemId.From(20), Name: InvoiceItemName.From("Post 1")),
                            new(Id: InvoiceItemId.From(21), Name: InvoiceItemName.From("Post 2")),
                        ]
                    ),
                    new(
                        Id: ProjectId.From(2),
                        Name: ProjectName.From("Project 2"),
                        Enabled: false,
                        Users: [],
                        InvoiceItems: []
                    ),
                ]
            )
        );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetProjectsMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(GetProjectsError.Unknown, GetAllProjectsUseCaseError.Unknown)]
    public async Task Execute_returns_errors_raised_by_repository_call(
        GetProjectsError repositoryError,
        GetAllProjectsUseCaseError expectedError
    )
    {
        var context = new GetAllProjectsUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetProjectsError(repositoryError);

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();

        errorResult.ShouldBe(expectedError);

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetProjectsMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        GetAllProjectsUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, GetAllProjectsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UnsupportedIdentityProvider, GetAllProjectsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserNotFound, GetAllProjectsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, GetAllProjectsUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        GetAllProjectsUseCaseError expectedError
    )
    {
        var context = new GetAllProjectsUseCaseTestContext().WithResolveUserError(resolveUserError);

        var useCase = context.BuildTarget();

        var result = await useCase.Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Fact]
    public async Task Execute_returns_error_for_unauthorized_user()
    {
        var context = new GetAllProjectsUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithProjectsPermission(UserPermission.None)
                .Build()
        );

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetAllProjectsUseCaseError.UnauthorizedUser);
    }
}

[GenerateTestContext(target: typeof(GetAllProjectsUseCase), GenerateWithMethods = true)]
internal partial class GetAllProjectsUseCaseTestContext
{
    public GetAllProjectsUseCaseTestContext WithGetProjectsSuccess(
        params GetProjectsResultProject[] projects
    ) => WithGetProjectsSuccess(new GetProjectsResult(Projects: projects));
}
