using Keepi.Core.Unit.Tests.Builders;
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
                            Id: 1,
                            Name: "Algemeen",
                            Enabled: true,
                            InvoiceItems: [new(Id: 10, Name: "Dev")]
                        ),
                        new(
                            Id: 2,
                            Name: "Intern",
                            Enabled: true,
                            InvoiceItems: [new(Id: 20, Name: "Administratie")]
                        ),
                    ],
                    Customizations:
                    [
                        new(InvoiceItemId: 10, Ordinal: 981, Color: Color.FromUint32(0xFF0000u)),
                        new(InvoiceItemId: 20, Ordinal: 982, Color: Color.FromUint32(0xFF00u)),
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
                        Id: 1,
                        Name: "Algemeen",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: 10,
                                Name: "Dev",
                                Customization: new(Ordinal: 981, Color: Color.FromUint32(0xFF0000u))
                            ),
                        ]
                    ),
                    new GetUserProjectsUseCaseOutputProject(
                        Id: 2,
                        Name: "Intern",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: 20,
                                Name: "Administratie",
                                Customization: new(Ordinal: 982, Color: Color.FromUint32(0xFF00u))
                            ),
                        ]
                    ),
                ]
            )
        );

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.GetUserProjectsMock.Verify(x => x.Execute(42, It.IsAny<CancellationToken>()));
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
                            Id: 1,
                            Name: "Algemeen",
                            Enabled: true,
                            InvoiceItems: [new(Id: 10, Name: "Dev")]
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
                        Id: 1,
                        Name: "Algemeen",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: 10,
                                Name: "Dev",
                                Customization: new(Ordinal: int.MaxValue, Color: null)
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

[GenerateTestContext(TargetType = typeof(GetUserProjectsUseCase), GenerateWithCallMethods = true)]
internal partial class GetUserProjectsUseCaseTestContext { }
