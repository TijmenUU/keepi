using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using Keepi.Core.Users;

namespace Keepi.Api.Users.UpdatePermissions;

internal sealed class UpdateUserPermissionsEndpoint(
    IUpdateUserPermissionsUseCase updateUserPermissionsUseCase
) : Endpoint<UpdateUserPermissionsRequest>
{
    public override void Configure()
    {
        Put("/users/{UserId}/permissions");
    }

    public override async Task HandleAsync(
        UpdateUserPermissionsRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = Route<int>(paramName: "UserId");

        if (TryGetValidatedModel(request, out var validatedRequest))
        {
            var result = await updateUserPermissionsUseCase.Execute(
                userId: userId,
                entriesPermission: validatedRequest.EntriesPermission,
                exportsPermission: validatedRequest.ExportsPermission,
                projectsPermission: validatedRequest.ProjectsPermission,
                usersPermission: validatedRequest.UsersPermission,
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
                    UpdateUserPermissionsUseCaseError.UnauthenticatedUser => Send.UnauthorizedAsync(
                        cancellation: cancellationToken
                    ),
                    UpdateUserPermissionsUseCaseError.UnauthorizedUser => Send.ForbiddenAsync(
                        cancellation: cancellationToken
                    ),
                    UpdateUserPermissionsUseCaseError.Unknown => Send.ErrorsAsync(
                        statusCode: 500,
                        cancellation: cancellationToken
                    ),
                    _ => Send.ErrorsAsync(cancellation: cancellationToken),
                }
            );
        }
        else
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
        }
    }

    private static bool TryGetValidatedModel(
        UpdateUserPermissionsRequest request,
        [NotNullWhen(returnValue: true)] out ValidatedUpdateUserPermissionsRequest? validated
    )
    {
        var entriesPermission = MapToDomainOrNull(request.EntriesPermission);
        var exportsPermission = MapToDomainOrNull(request.ExportsPermission);
        var projectsPermission = MapToDomainOrNull(request.ProjectsPermission);
        var usersPermission = MapToDomainOrNull(request.UsersPermission);
        if (
            entriesPermission == null
            || exportsPermission == null
            || projectsPermission == null
            || usersPermission == null
        )
        {
            validated = null;
            return false;
        }

        validated = new ValidatedUpdateUserPermissionsRequest(
            EntriesPermission: entriesPermission.Value,
            ExportsPermission: exportsPermission.Value,
            ProjectsPermission: projectsPermission.Value,
            UsersPermission: usersPermission.Value
        );
        return true;
    }

    private static UserPermission? MapToDomainOrNull(UpdateUserPermissionsRequestPermission? value)
    {
        return value switch
        {
            UpdateUserPermissionsRequestPermission.None => UserPermission.None,
            UpdateUserPermissionsRequestPermission.Read => UserPermission.Read,
            UpdateUserPermissionsRequestPermission.ReadAndModify => UserPermission.ReadAndModify,
            _ => null,
        };
    }

    record ValidatedUpdateUserPermissionsRequest(
        UserPermission EntriesPermission,
        UserPermission ExportsPermission,
        UserPermission ProjectsPermission,
        UserPermission UsersPermission
    );
}
