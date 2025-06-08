using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Api.UserEntryCategories.GetAll;
using Keepi.Core.UserEntryCategories;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntryCategories.Create;

public class PostCreateUserUserEntryCategoryEndpoint(
    IResolveUserHelper resolveUserHelper,
    ICreateUserEntryCategoryUseCase createUserEntryCategoryUseCase,
    ILogger<PostCreateUserUserEntryCategoryEndpoint> logger
) : Endpoint<PostCreateUserUserEntryCategoryRequest, PostCreateUserUserEntryCategoryResponse>
{
    public override void Configure()
    {
        Post("/user/entrycategories");
    }

    public override async Task HandleAsync(
        PostCreateUserUserEntryCategoryRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to create entry category for unregistered user");
            await SendForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        if (!TryGetValidatedModel(request, out var validatedRequest))
        {
            await SendErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var result = await createUserEntryCategoryUseCase.Execute(
            userId: user.Id,
            name: validatedRequest.Name,
            enabled: validatedRequest.Enabled,
            activeFrom: validatedRequest.ActiveFrom,
            activeTo: validatedRequest.ActiveTo,
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var success, out var error))
        {
            await SendCreatedAtAsync<GetUserUserEntryCategoriesEndpoint>(
                responseBody: new PostCreateUserUserEntryCategoryResponse(
                    id: success.UserEntryCategoryId
                ),
                cancellation: cancellationToken
            );
            return;
        }

        if (error == CreateUserEntryCategoryUseCaseError.Unknown)
        {
            await SendErrorsAsync(statusCode: 500, cancellation: cancellationToken);
            return;
        }

        await SendErrorsAsync(cancellation: cancellationToken);
        return;
    }

    private static bool TryGetValidatedModel(
        PostCreateUserUserEntryCategoryRequest request,
        [NotNullWhen(returnValue: true)] out ValidatedPostCreateUserEntryCategoryRequest? validated
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

        validated = new ValidatedPostCreateUserEntryCategoryRequest(
            Name: request.Name,
            Enabled: request.Enabled.Value,
            ActiveFrom: parsedActiveFrom,
            ActiveTo: parsedActiveTo
        );
        return true;
    }

    record ValidatedPostCreateUserEntryCategoryRequest(
        string Name,
        bool Enabled,
        DateOnly? ActiveFrom,
        DateOnly? ActiveTo
    );
}
