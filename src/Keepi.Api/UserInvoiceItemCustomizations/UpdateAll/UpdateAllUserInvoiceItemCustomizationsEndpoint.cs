using FastEndpoints;
using Keepi.Core.InvoiceItems;
using Keepi.Core.UserInvoiceItemCustomizations;

namespace Keepi.Api.UserInvoiceItemCustomizations.UpdateAll;

public sealed class UpdateAllUserInvoiceItemCustomizationsEndpoint(
    IUpdateUserInvoiceCustomizationsUseCase updateUserInvoiceCustomizationsUseCase
) : Endpoint<UpdateAllUserInvoiceItemCustomizationsRequest>
{
    public override void Configure()
    {
        Put("/user/invoiceitemcustomizations");
    }

    public override async Task HandleAsync(
        UpdateAllUserInvoiceItemCustomizationsRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!TryGetValidatedModel(request: request, out var validatedRequest))
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var result = await updateUserInvoiceCustomizationsUseCase.Execute(
            new(InvoiceItems: validatedRequest),
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var errorResult))
        {
            await Send.NoContentAsync(cancellation: cancellationToken);
            return;
        }

        await (
            errorResult switch
            {
                UpdateUserInvoiceCustomizationsUseCaseError.UnauthenticatedUser =>
                    Send.UnauthorizedAsync(cancellation: cancellationToken),
                UpdateUserInvoiceCustomizationsUseCaseError.UnauthorizedUser => Send.ForbiddenAsync(
                    cancellation: cancellationToken
                ),
                UpdateUserInvoiceCustomizationsUseCaseError.Unknown => Send.ErrorsAsync(
                    statusCode: 500,
                    cancellation: cancellationToken
                ),
                _ => Send.ErrorsAsync(cancellation: cancellationToken),
            }
        );
    }

    private static bool TryGetValidatedModel(
        UpdateAllUserInvoiceItemCustomizationsRequest request,
        out UpdateUserInvoiceCustomizationsUseCaseInputInvoiceItem[] validatedRequest
    )
    {
        if (request.InvoiceItems == null)
        {
            validatedRequest = [];
            return false;
        }

        var result = new List<UpdateUserInvoiceCustomizationsUseCaseInputInvoiceItem>();
        foreach (var item in request.InvoiceItems)
        {
            if (item == null || item.Id == null || item.Ordinal == null)
            {
                validatedRequest = [];
                return false;
            }

            if (
                !InvoiceItemId.TryFrom(value: item.Id.Value, out var invoiceItemId)
                || !UserInvoiceITemCustomizationOrdinal.TryFrom(
                    value: item.Ordinal.Value,
                    out var ordinal
                )
            )
            {
                validatedRequest = [];
                return false;
            }

            if (item.Color == null)
            {
                result.Add(new(InvoiceItemId: invoiceItemId, Ordinal: ordinal, Color: null));
            }
            else
            {
                if (Core.Color.TryParseHexString(item.Color, out var color))
                {
                    result.Add(new(InvoiceItemId: invoiceItemId, Ordinal: ordinal, Color: color));
                }
                else
                {
                    validatedRequest = [];
                    return false;
                }
            }
        }

        validatedRequest = [.. result];
        return true;
    }
}
