using Keepi.Core.Users;

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
    UnauthenticatedUser,
    DuplicateInvoiceItemId,
    UnknownInvoiceItemId,
    UnauthorizedUser,
}

public record UpdateUserInvoiceCustomizationsUseCaseInput(
    UpdateUserInvoiceCustomizationsUseCaseInputInvoiceItem[] InvoiceItems
);

public record UpdateUserInvoiceCustomizationsUseCaseInputInvoiceItem(
    int InvoiceItemId,
    int Ordinal,
    Color? Color
);

internal sealed class UpdateUserInvoiceCustomizationsUseCase(
    IResolveUser resolveUser,
    IOverwriteUserInvoiceItemCustomizations overwriteUserInvoiceItemCustomizations
) : IUpdateUserInvoiceCustomizationsUseCase
{
    public async Task<IMaybeErrorResult<UpdateUserInvoiceCustomizationsUseCaseError>> Execute(
        UpdateUserInvoiceCustomizationsUseCaseInput input,
        CancellationToken cancellationToken
    )
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out var userSuccessResult, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure(
                    UpdateUserInvoiceCustomizationsUseCaseError.UnauthenticatedUser
                ),
                _ => Result.Failure(UpdateUserInvoiceCustomizationsUseCaseError.Unknown),
            };
        }
        if (!userSuccessResult.EntriesPermission.CanModify())
        {
            return Result.Failure(UpdateUserInvoiceCustomizationsUseCaseError.UnauthorizedUser);
        }

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
                UserId: userSuccessResult.Id,
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
