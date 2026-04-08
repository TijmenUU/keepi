using Keepi.Core.InvoiceItems;
using Keepi.Core.Users;

namespace Keepi.Core.Projects;

public interface ISaveNewProject
{
    Task<IValueOrErrorResult<int, SaveNewProjectError>> Execute(
        ProjectName name,
        bool enabled,
        UserId[] userIds,
        InvoiceItemName[] invoiceItemNames,
        CancellationToken cancellationToken
    );
}

public enum SaveNewProjectError
{
    Unknown = 0,
    DuplicateProjectName,
    UnknownUserId,
}
