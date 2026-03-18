using Keepi.Core.Entries;
using Keepi.Core.InvoiceItems;
using Keepi.Core.Users;

namespace Keepi.Core.Projects;

public interface IUpdateProject
{
    Task<IMaybeErrorResult<UpdateProjectError>> Execute(
        ProjectId id,
        ProjectName name,
        bool enabled,
        UserId[] userIds,
        (InvoiceItemId? Id, InvoiceItemName Name)[] invoiceItems,
        CancellationToken cancellationToken
    );
}

public enum UpdateProjectError
{
    Unknown = 0,
    UnknownProjectId,
    DuplicateProjectName,
    UnknownUserId,
}
