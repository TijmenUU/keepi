namespace Keepi.Core.UserProjects;

public interface IGetUserProjects
{
    Task<IValueOrErrorResult<GetUserProjectResult, GetUserProjectsError>> Execute(
        int userId,
        CancellationToken cancellationToken
    );
}

public enum GetUserProjectsError
{
    Unknown = 0,
}

public sealed record GetUserProjectResult(
    GetUserProjectResultProject[] Projects,
    GetUserProjectResultInvoiceItemCustomization[] Customizations
);

public sealed record GetUserProjectResultProject(
    int Id,
    string Name,
    bool Enabled,
    GetUserProjectResultProjectInvoiceItem[] InvoiceItems
);

public sealed record GetUserProjectResultProjectInvoiceItem(int Id, string Name);

public sealed record GetUserProjectResultInvoiceItemCustomization(
    int InvoiceItemId,
    int Ordinal,
    Color? Color
);
