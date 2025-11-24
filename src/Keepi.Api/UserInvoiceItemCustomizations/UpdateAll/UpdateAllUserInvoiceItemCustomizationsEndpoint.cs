using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.UserInvoiceItemCustomizations;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserInvoiceItemCustomizations.UpdateAll;

public sealed class UpdateAllUserInvoiceItemCustomizationsEndpoint(
    IResolveUserHelper resolveUserHelper,
    IUpdateUserInvoiceCustomizationsUseCase updateUserInvoiceCustomizationsUseCase,
    ILogger<UpdateAllUserInvoiceItemCustomizationsEndpoint> logger
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
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to update user invoice item customizations for unknown user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        if (!TryGetValidatedModel(request: request, out var validatedRequest))
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var result = await updateUserInvoiceCustomizationsUseCase.Execute(
            new(UserId: user.Id, InvoiceItems: validatedRequest),
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var error))
        {
            await Send.NoContentAsync(cancellation: cancellationToken);
            return;
        }

        if (error == UpdateUserInvoiceCustomizationsUseCaseError.Unknown)
        {
            await Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken);
            return;
        }

        await Send.ErrorsAsync(cancellation: cancellationToken);
        return;
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

            if (item.Color == null)
            {
                result.Add(
                    new(InvoiceItemId: item.Id.Value, Ordinal: item.Ordinal.Value, Color: null)
                );
            }
            else
            {
                if (Core.Color.TryParseHexString(item.Color, out var color))
                {
                    result.Add(
                        new(InvoiceItemId: item.Id.Value, Ordinal: item.Ordinal.Value, Color: color)
                    );
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
