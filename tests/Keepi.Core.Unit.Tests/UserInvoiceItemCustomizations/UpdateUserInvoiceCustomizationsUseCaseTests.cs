using Keepi.Core.Unit.Tests.Builders;
using Keepi.Core.UserInvoiceItemCustomizations;
using Keepi.Core.Users;
using Keepi.Generators;

namespace Keepi.Core.Unit.Tests.UserInvoiceItemCustomizations;

public class UpdateUserInvoiceCustomizationsUseCaseTests
{
    [Fact]
    public async Task Execute_returns_success()
    {
        var context = new UpdateUserInvoiceCustomizationsUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithOverwriteUserInvoiceItemCustomizationsSuccess();

        var result = await context
            .BuildTarget()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    InvoiceItems:
                    [
                        new(
                            InvoiceItemId: 1,
                            Ordinal: 100,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                        new(InvoiceItemId: 2, Ordinal: 200, Color: Color.FromUint32(0xFF00u)),
                    ]
                ),
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out _).ShouldBeTrue();

        context.ResolveUserMock.Verify(x => x.Execute(It.IsAny<CancellationToken>()));
        context.OverwriteUserInvoiceItemCustomizationsMock.Verify(x =>
            x.Execute(
                It.Is<OverwriteUserInvoiceItemCustomizationsInput>(i =>
                    i.UserId == 42
                    && i.InvoiceItems.Length == 2
                    && i.InvoiceItems[0].InvoiceItemId == 1
                    && i.InvoiceItems[0].Ordinal == 100
                    && i.InvoiceItems[0].Color != null
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    && i.InvoiceItems[0].Color.Red == 255
                    && i.InvoiceItems[0].Color.Green == 255
                    && i.InvoiceItems[0].Color.Blue == 255
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    && i.InvoiceItems[1].InvoiceItemId == 2
                    && i.InvoiceItems[1].Ordinal == 200
                    && i.InvoiceItems[1].Color != null
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    && i.InvoiceItems[1].Color.Red == 0
                    && i.InvoiceItems[1].Color.Green == 255
                    && i.InvoiceItems[1].Color.Blue == 0
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                ),
                It.IsAny<CancellationToken>()
            )
        );
        context.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Execute_returns_error_for_duplicate_invoice_item_ids()
    {
        var context =
            new UpdateUserInvoiceCustomizationsUseCaseTestContext().WithResolveUserSuccess(
                ResolvedUserBuilder.CreateAdministratorBob()
            );

        var result = await context
            .BuildTarget()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    InvoiceItems:
                    [
                        new(
                            InvoiceItemId: 1,
                            Ordinal: 100,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                        new(InvoiceItemId: 1, Ordinal: 200, Color: Color.FromUint32(0xFF00u)),
                    ]
                ),
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateUserInvoiceCustomizationsUseCaseError.DuplicateInvoiceItemId);
    }

    [Fact]
    public async Task Execute_returns_unknown_invoice_item_id_overwrite_customizations_error()
    {
        var context = new UpdateUserInvoiceCustomizationsUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithOverwriteUserInvoiceItemCustomizationsError(
                OverwriteUserInvoiceItemCustomizationsError.UnknownInvoiceItemId
            );

        var result = await context
            .BuildTarget()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    InvoiceItems:
                    [
                        new(
                            InvoiceItemId: 1,
                            Ordinal: 100,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                        new(InvoiceItemId: 2, Ordinal: 200, Color: Color.FromUint32(0xFF00u)),
                    ]
                ),
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateUserInvoiceCustomizationsUseCaseError.UnknownInvoiceItemId);
    }

    [Fact]
    public async Task Execute_returns_unknown_overwrite_customizations_error()
    {
        var context = new UpdateUserInvoiceCustomizationsUseCaseTestContext()
            .WithResolveUserSuccess(ResolvedUserBuilder.CreateAdministratorBob())
            .WithOverwriteUserInvoiceItemCustomizationsError(
                OverwriteUserInvoiceItemCustomizationsError.Unknown
            );

        var result = await context
            .BuildTarget()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    InvoiceItems:
                    [
                        new(
                            InvoiceItemId: 1,
                            Ordinal: 100,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                        new(InvoiceItemId: 2, Ordinal: 200, Color: Color.FromUint32(0xFF00u)),
                    ]
                ),
                cancellationToken: CancellationToken.None
            );

        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateUserInvoiceCustomizationsUseCaseError.Unknown);
    }

    [Theory]
    [InlineData(
        ResolveUserError.UserNotAuthenticated,
        UpdateUserInvoiceCustomizationsUseCaseError.UnauthenticatedUser
    )]
    [InlineData(
        ResolveUserError.MalformedUserClaims,
        UpdateUserInvoiceCustomizationsUseCaseError.Unknown
    )]
    [InlineData(
        ResolveUserError.UnsupportedIdentityProvider,
        UpdateUserInvoiceCustomizationsUseCaseError.Unknown
    )]
    [InlineData(ResolveUserError.UserNotFound, UpdateUserInvoiceCustomizationsUseCaseError.Unknown)]
    [InlineData(
        ResolveUserError.UserRegistrationFailed,
        UpdateUserInvoiceCustomizationsUseCaseError.Unknown
    )]
    public async Task Execute_returns_error_for_user_resolve_error(
        ResolveUserError resolveUserError,
        UpdateUserInvoiceCustomizationsUseCaseError expectedError
    )
    {
        var context = new UpdateUserInvoiceCustomizationsUseCaseTestContext().WithResolveUserError(
            resolveUserError
        );

        var result = await context
            .BuildTarget()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    InvoiceItems:
                    [
                        new(
                            InvoiceItemId: 1,
                            Ordinal: 100,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                        new(InvoiceItemId: 2, Ordinal: 200, Color: Color.FromUint32(0xFF00u)),
                    ]
                ),
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(expectedError);
    }

    [Theory]
    [InlineData(UserPermission.None)]
    [InlineData(UserPermission.Read)]
    public async Task Execute_returns_error_for_unauthorized_user(UserPermission entriesPermission)
    {
        var context =
            new UpdateUserInvoiceCustomizationsUseCaseTestContext().WithResolveUserSuccess(
                ResolvedUserBuilder
                    .AsAdministratorBob()
                    .WithEntriesPermission(entriesPermission)
                    .Build()
            );

        var result = await context
            .BuildTarget()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    InvoiceItems:
                    [
                        new(
                            InvoiceItemId: 1,
                            Ordinal: 100,
                            Color: new(Red: 255, Green: 255, Blue: 255)
                        ),
                        new(InvoiceItemId: 2, Ordinal: 200, Color: Color.FromUint32(0xFF00u)),
                    ]
                ),
                cancellationToken: CancellationToken.None
            );
        result.TrySuccess(out var errorResult).ShouldBeFalse();
        errorResult.ShouldBe(UpdateUserInvoiceCustomizationsUseCaseError.UnauthorizedUser);
    }
}

[GenerateTestContext(
    TargetType = typeof(UpdateUserInvoiceCustomizationsUseCase),
    GenerateWithCallMethods = true
)]
internal partial class UpdateUserInvoiceCustomizationsUseCaseTestContext { }
