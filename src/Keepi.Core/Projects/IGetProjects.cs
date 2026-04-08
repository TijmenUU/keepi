using Keepi.Core.InvoiceItems;
using Keepi.Core.Users;

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
    ProjectId Id,
    ProjectName Name,
    bool Enabled,
    GetProjectsResultProjectUser[] Users,
    GetProjectsResultProjectInvoiceItem[] InvoiceItems
);

public sealed record GetProjectsResultProjectUser(UserId Id, UserName Name);

public sealed record GetProjectsResultProjectInvoiceItem(InvoiceItemId Id, InvoiceItemName Name);
