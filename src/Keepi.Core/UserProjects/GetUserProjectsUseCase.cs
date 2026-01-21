using Keepi.Core.Users;

namespace Keepi.Core.UserProjects;

public interface IGetUserProjectsUseCase
{
    Task<IValueOrErrorResult<GetUserProjectsUseCaseOutput, GetUserProjectsUseCaseError>> Execute(
        CancellationToken cancellationToken
    );
}

public enum GetUserProjectsUseCaseError
{
    Unknown = 0,
    UnauthenticatedUser,
    UnauthorizedUser,
}

public sealed record GetUserProjectsUseCaseOutput(GetUserProjectsUseCaseOutputProject[] Projects);

public sealed record GetUserProjectsUseCaseOutputProject(
    int Id,
    string Name,
    bool Enabled,
    GetUserProjectsUseCaseOutputProjectInvoiceItem[] InvoiceItems
);

public sealed record GetUserProjectsUseCaseOutputProjectInvoiceItem(
    int Id,
    string Name,
    GetUserProjectsUseCaseOutputProjectInvoiceItemCustomization Customization
);

public sealed record GetUserProjectsUseCaseOutputProjectInvoiceItemCustomization(
    int Ordinal,
    Color? Color
);

internal class GetUserProjectsUseCase(IResolveUser resolveUser, IGetUserProjects getUserProjects)
    : IGetUserProjectsUseCase
{
    public async Task<
        IValueOrErrorResult<GetUserProjectsUseCaseOutput, GetUserProjectsUseCaseError>
    > Execute(CancellationToken cancellationToken)
    {
        var userResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!userResult.TrySuccess(out var userSuccessResult, out var userErrorResult))
        {
            return userErrorResult switch
            {
                ResolveUserError.UserNotAuthenticated => Result.Failure<
                    GetUserProjectsUseCaseOutput,
                    GetUserProjectsUseCaseError
                >(GetUserProjectsUseCaseError.UnauthenticatedUser),
                _ => Result.Failure<GetUserProjectsUseCaseOutput, GetUserProjectsUseCaseError>(
                    GetUserProjectsUseCaseError.Unknown
                ),
            };
        }
        if (!userSuccessResult.EntriesPermission.CanRead())
        {
            return Result.Failure<GetUserProjectsUseCaseOutput, GetUserProjectsUseCaseError>(
                GetUserProjectsUseCaseError.UnauthorizedUser
            );
        }

        var result = await getUserProjects.Execute(
            userId: userSuccessResult.Id,
            cancellationToken: cancellationToken
        );

        if (!result.TrySuccess(out var successResult, out _))
        {
            return Result.Failure<GetUserProjectsUseCaseOutput, GetUserProjectsUseCaseError>(
                GetUserProjectsUseCaseError.Unknown
            );
        }

        var userProjects = new List<GetUserProjectsUseCaseOutputProject>();
        foreach (var project in successResult.Projects)
        {
            var invoiceItems = new List<GetUserProjectsUseCaseOutputProjectInvoiceItem>();
            foreach (var item in project.InvoiceItems)
            {
                var customization =
                    successResult.Customizations.FirstOrDefault(c => c.InvoiceItemId == item.Id)
                    ?? new GetUserProjectResultInvoiceItemCustomization(
                        InvoiceItemId: item.Id,
                        Ordinal: int.MaxValue,
                        Color: null
                    );
                invoiceItems.Add(
                    new(
                        Id: customization.InvoiceItemId,
                        Name: item.Name,
                        Customization: new GetUserProjectsUseCaseOutputProjectInvoiceItemCustomization(
                            Ordinal: customization.Ordinal,
                            Color: customization.Color
                        )
                    )
                );
            }

            userProjects.Add(
                new(
                    Id: project.Id,
                    Name: project.Name,
                    Enabled: project.Enabled,
                    InvoiceItems: [.. invoiceItems]
                )
            );
        }

        return Result.Success<GetUserProjectsUseCaseOutput, GetUserProjectsUseCaseError>(
            new(Projects: [.. userProjects])
        );
    }
}
