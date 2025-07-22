using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.UserEntryCategories;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntryCategories.UpdateAll;

public class PutUpdateUserEntryCategoriesEndpoint(
    IResolveUserHelper resolveUserHelper,
    IUpdateUserEntryCategoriesUseCase updateUserEntryCategoriesUseCase,
    ILogger<PutUpdateUserEntryCategoriesEndpoint> logger
) : Endpoint<PutUpdateUserEntryCategoriesRequest>
{
    public override void Configure()
    {
        Put("/user/entrycategories");
    }

    public override async Task HandleAsync(
        PutUpdateUserEntryCategoriesRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to create entry categories for unregistered user");
            await SendForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        if (!TryGetValidatedModel(request, out var validatedRequest))
        {
            await SendErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var result = await updateUserEntryCategoriesUseCase.Execute(
            userId: user.Id,
            userEntryCategories: validatedRequest
                .Select(r => new UpdateUserEntryCategoriesUseCaseInput(
                    Id: r.Id,
                    Name: r.Name,
                    Ordinal: r.Ordinal,
                    Enabled: r.Enabled,
                    ActiveFrom: r.ActiveFrom,
                    ActiveTo: r.ActiveTo
                ))
                .ToArray(),
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var error))
        {
            await SendNoContentAsync(cancellation: cancellationToken);
            return;
        }

        if (error == UpdateUserEntryCategoriesUseCaseError.UserEntryCategoryDoesNotExist)
        {
            await SendErrorsAsync(statusCode: 404, cancellation: cancellationToken);
            return;
        }
        if (error == UpdateUserEntryCategoriesUseCaseError.Unknown)
        {
            await SendErrorsAsync(statusCode: 500, cancellation: cancellationToken);
            return;
        }

        await SendErrorsAsync(cancellation: cancellationToken);
        return;
    }

    private static bool TryGetValidatedModel(
        PutUpdateUserEntryCategoriesRequest request,
        [NotNullWhen(returnValue: true)] out ValidatedPostCreateUserEntryCategory[]? validated
    )
    {
        if (request == null || request.UserEntryCategories == null)
        {
            validated = null;
            return false;
        }

        List<ValidatedPostCreateUserEntryCategory> validatedCategories = [];
        foreach (var category in request.UserEntryCategories)
        {
            if (!TryGetValidatedModel(category, out var validatedCategory))
            {
                validated = null;
                return false;
            }
            validatedCategories.Add(validatedCategory);
        }

        validated = [.. validatedCategories];
        return true;
    }

    private static bool TryGetValidatedModel(
        PutUpdateUserEntryCategoriesRequestCategory? request,
        [NotNullWhen(returnValue: true)] out ValidatedPostCreateUserEntryCategory? validated
    )
    {
        if (request == null)
        {
            validated = null;
            return false;
        }

        if (!UserEntryCategoryEntity.IsValidName(request.Name))
        {
            validated = null;
            return false;
        }

        if (!UserEntryCategoryEntity.IsValidOrdinal(request.Ordinal))
        {
            validated = null;
            return false;
        }

        if (request.Enabled == null)
        {
            validated = null;
            return false;
        }

        DateOnly? parsedActiveFrom = null;
        if (request.ActiveFrom != null)
        {
            parsedActiveFrom = request.ActiveFrom.GetIsoDateOrNull();
            if (parsedActiveFrom == null)
            {
                validated = null;
                return false;
            }
        }

        DateOnly? parsedActiveTo = null;
        if (request.ActiveTo != null)
        {
            parsedActiveTo = request.ActiveTo.GetIsoDateOrNull();
            if (parsedActiveTo == null)
            {
                validated = null;
                return false;
            }
        }

        if (
            !UserEntryCategoryEntity.IsValidActiveDateRange(
                from: parsedActiveFrom,
                to: parsedActiveTo
            )
        )
        {
            validated = null;
            return false;
        }

        validated = new ValidatedPostCreateUserEntryCategory(
            Id: request.Id,
            Name: request.Name,
            Ordinal: request.Ordinal.Value,
            Enabled: request.Enabled.Value,
            ActiveFrom: parsedActiveFrom,
            ActiveTo: parsedActiveTo
        );
        return true;
    }

    record ValidatedPostCreateUserEntryCategory(
        int? Id,
        string Name,
        int Ordinal,
        bool Enabled,
        DateOnly? ActiveFrom,
        DateOnly? ActiveTo
    );
}
