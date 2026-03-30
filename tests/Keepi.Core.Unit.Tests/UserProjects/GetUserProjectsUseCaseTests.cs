using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.UserInvoiceItemCustomizations;
using Keepi.Core.UserProjects;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.UserProjects;

public class GetUserProjectsUseCaseTests
{
    [Fact]
    public async Task Execute_returns_success_output()
    {
        var context = new GetUserProjectsUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsSuccess(
                new(
                    Projects:
                    [
                        new(
                            Id: ProjectId.From(1),
                            Name: ProjectName.From("Algemeen"),
                            Enabled: true,
                            InvoiceItems:
                            [
                                new(Id: InvoiceItemId.From(10), Name: InvoiceItemName.From("Dev")),
                            ]
                        ),
                        new(
                            Id: ProjectId.From(2),
                            Name: ProjectName.From("Intern"),
                            Enabled: true,
                            InvoiceItems:
                            [
                                new(
                                    Id: InvoiceItemId.From(20),
                                    Name: InvoiceItemName.From("Administratie")
                                ),
                            ]
                        ),
                    ],
                    Customizations:
                    [
                        new(
                            InvoiceItemId: InvoiceItemId.From(10),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(981),
                            Color: Color.From(0xFF0000u)
                        ),
                        new(
                            InvoiceItemId: InvoiceItemId.From(20),
                            Ordinal: UserInvoiceITemCustomizationOrdinal.From(982),
                            Color: Color.From(0xFF00u)
                        ),
                    ]
                )
            );

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBeEquivalentTo(
            new GetUserProjectsUseCaseOutput(
                Projects:
                [
                    new GetUserProjectsUseCaseOutputProject(
                        Id: ProjectId.From(1),
                        Name: ProjectName.From("Algemeen"),
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: InvoiceItemId.From(10),
                                Name: InvoiceItemName.From("Dev"),
                                Customization: new(
                                    Ordinal: UserInvoiceITemCustomizationOrdinal.From(981),
                                    Color: Color.From(0xFF0000u)
                                )
                            ),
                        ]
                    ),
                    new GetUserProjectsUseCaseOutputProject(
                        Id: ProjectId.From(2),
                        Name: ProjectName.From("Intern"),
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: InvoiceItemId.From(20),
                                Name: InvoiceItemName.From("Administratie"),
                                Customization: new(
                                    Ordinal: UserInvoiceITemCustomizationOrdinal.From(982),
                                    Color: Color.From(0xFF00u)
                                )
                            ),
                        ]
                    ),
                ]
            )
        );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetUserProjectsMock.Verify(x =>
            x.Execute(UserId.From(42), It.IsAny<CancellationToken>())
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_success_with_default_customization()
    {
        var context = new GetUserProjectsUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsSuccess(
                new(
                    Projects:
                    [
                        new(
                            Id: ProjectId.From(1),
                            Name: ProjectName.From("Algemeen"),
                            Enabled: true,
                            InvoiceItems:
                            [
                                new(Id: InvoiceItemId.From(10), Name: InvoiceItemName.From("Dev")),
                            ]
                        ),
                    ],
                    Customizations: []
                )
            );

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out var successResult, out _).ShouldBeTrue();
        successResult.ShouldBeEquivalentTo(
            new GetUserProjectsUseCaseOutput(
                Projects:
                [
                    new GetUserProjectsUseCaseOutputProject(
                        Id: ProjectId.From(1),
                        Name: ProjectName.From("Algemeen"),
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: InvoiceItemId.From(10),
                                Name: InvoiceItemName.From("Dev"),
                                Customization: new(
                                    Ordinal: UserInvoiceITemCustomizationOrdinal.From(int.MaxValue),
                                    Color: null
                                )
                            ),
                        ]
                    ),
                ]
            )
        );
    }

    [Fact]
    public async Task Execute_returns_unknown_get_user_projects_error()
    {
        var context = new GetUserProjectsUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithGetUserProjectsError(GetUserProjectsError.Unknown);

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetUserProjectsUseCaseError.Unknown);
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        GetUserProjectsUseCaseError.UnauthenticatedUser
    )]
    [InlineData(ResolveUserError.MalformedUserClaims, GetUserProjectsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UnsupportedIdentityProvider, GetUserProjectsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserNotFound, GetUserProjectsUseCaseError.Unknown)]
    [InlineData(ResolveUserError.UserRegistrationFailed, GetUserProjectsUseCaseError.Unknown)]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        GetUserProjectsUseCaseError expectedError
    )
    {
        var context = new GetUserProjectsUseCaseTestContext().WithResolveUserError(
            resolveUserError
        );

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);

        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Fact]
    public async Task Execute_returns_error_for_unauthorized_user()
    {
        var context = new GetUserProjectsUseCaseTestContext().WithResolveUserSuccess(
            ResolvedUserBuilder
                .AsAdministratorBob()
                .WithEntriesPermission(UserPermission.None)
                .Build()
        );

        var result = await context.BuildTarget().Execute(cancellationToken: CancellationToken.None);
        result.TrySuccess(out _, out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(GetUserProjectsUseCaseError.UnauthorizedUser);
    }
}

[GenerateTestContext(targetType: typeof(GetUserProjectsUseCase), GenerateWithMethods = true)]
internal partial class GetUserProjectsUseCaseTestContext { }
