namespace Keepi.Core.UserInvoiceItemCustomizations;

public interface IUpdateUserInvoiceCustomizationsUseCase
{
    Task<IMaybeErrorResult<UpdateUserInvoiceCustomizationsUseCaseError>> Execute(
        UpdateUserInvoiceCustomizationsUseCaseInput input,
        CancellationToken cancellationToken
    );
}

public enum UpdateUserInvoiceCustomizationsUseCaseError
{
    Unknown = 0,
    DuplicateInvoiceItemId,
    UnknownInvoiceItemId,
}

public record UpdateUserInvoiceCustomizationsUseCaseInput(
    int UserId,
    UpdateUserInvoiceCustomizationsUseCaseInputInvoiceItem[] InvoiceItems
);

public record UpdateUserInvoiceCustomizationsUseCaseInputInvoiceItem(
    int InvoiceItemId,
    int Ordinal,
    Color? Color
);

internal sealed class UpdateUserInvoiceCustomizationsUseCase(
    IOverwriteUserInvoiceItemCustomizations overwriteUserInvoiceItemCustomizations
) : IUpdateUserInvoiceCustomizationsUseCase
{
    public async Task<IMaybeErrorResult<UpdateUserInvoiceCustomizationsUseCaseError>> Execute(
        UpdateUserInvoiceCustomizationsUseCaseInput input,
        CancellationToken cancellationToken
    )
    {
        if (
            input.InvoiceItems.DistinctBy(i => i.InvoiceItemId).Count() != input.InvoiceItems.Length
        )
        {
            return Result.Failure(
                UpdateUserInvoiceCustomizationsUseCaseError.DuplicateInvoiceItemId
            );
        }

        var result = await overwriteUserInvoiceItemCustomizations.Execute(
            input: new(
                UserId: input.UserId,
                InvoiceItems: input
                    .InvoiceItems.Select(
                        i => new OverwriteUserInvoiceItemCustomizationsInputInvoiceItem(
                            InvoiceItemId: i.InvoiceItemId,
                            Ordinal: i.Ordinal,
                            Color: i.Color
                        )
                    )
                    .ToArray()
            ),
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var errorResult))
        {
            return Result.Success<UpdateUserInvoiceCustomizationsUseCaseError>();
        }

        if (errorResult == OverwriteUserInvoiceItemCustomizationsError.UnknownInvoiceItemId)
        {
            return Result.Failure(UpdateUserInvoiceCustomizationsUseCaseError.UnknownInvoiceItemId);
        }

        return Result.Failure(UpdateUserInvoiceCustomizationsUseCaseError.Unknown);
    }
}
