using Keepi.Core.UserInvoiceItemCustomizations;

namespace Keepi.Core.Unit.Tests.UserInvoiceItemCustomizations;

public class UpdateUserInvoiceCustomizationsUseCaseTests
{
    [Fact]
    public async Task Execute_returns_success()
    {
        var context = new TestContext().WithOverwriteCustomizationsSuccessResult();

        var result = await context
            .BuildUseCase()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    UserId: 42,
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
    }

    [Fact]
    public async Task Execute_returns_error_for_duplicate_invoice_item_ids()
    {
        var context = new TestContext();

        var result = await context
            .BuildUseCase()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    UserId: 42,
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
        var context = new TestContext().WithOverwriteCustomizationsFailureResult(
            OverwriteUserInvoiceItemCustomizationsError.UnknownInvoiceItemId
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    UserId: 42,
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
        var context = new TestContext().WithOverwriteCustomizationsFailureResult(
            OverwriteUserInvoiceItemCustomizationsError.Unknown
        );

        var result = await context
            .BuildUseCase()
            .Execute(
                input: new UpdateUserInvoiceCustomizationsUseCaseInput(
                    UserId: 42,
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

    private class TestContext
    {
        public Mock<IOverwriteUserInvoiceItemCustomizations> OverwriteUserInvoiceItemCustomizationsMock { get; } =
            new Mock<IOverwriteUserInvoiceItemCustomizations>(MockBehavior.Strict);

        public TestContext WithOverwriteCustomizationsSuccessResult()
        {
            OverwriteUserInvoiceItemCustomizationsMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<OverwriteUserInvoiceItemCustomizationsInput>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Success<OverwriteUserInvoiceItemCustomizationsError>());

            return this;
        }

        public TestContext WithOverwriteCustomizationsFailureResult(
            OverwriteUserInvoiceItemCustomizationsError result
        )
        {
            OverwriteUserInvoiceItemCustomizationsMock
                .Setup(x =>
                    x.Execute(
                        It.IsAny<OverwriteUserInvoiceItemCustomizationsInput>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result.Failure(result));

            return this;
        }

        public UpdateUserInvoiceCustomizationsUseCase BuildUseCase() =>
            new(
                overwriteUserInvoiceItemCustomizations: OverwriteUserInvoiceItemCustomizationsMock.Object
            );
    }
}
