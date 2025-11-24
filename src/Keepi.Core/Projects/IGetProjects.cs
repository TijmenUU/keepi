namespace Keepi.Core.Projects;

public interface IGetProjects
{
    Task<IValueOrErrorResult<GetProjectsResult, GetProjectsError>> Execute(
        CancellationToken cancellationToken
    );
}

public enum GetProjectsError
{
    Unknown = 0,
}

public sealed record GetProjectsResult(GetProjectsResultProject[] Projects);

public sealed record GetProjectsResultProject(
    int Id,
    string Name,
    bool Enabled,
    GetProjectsResultProjectUser[] Users,
    GetProjectsResultProjectInvoiceItem[] InvoiceItems
);

public sealed record GetProjectsResultProjectUser(int Id, string Name);

public sealed record GetProjectsResultProjectInvoiceItem(int Id, string Name);
