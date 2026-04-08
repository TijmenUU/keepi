using Keepi.Core.InvoiceItems;
using Keepi.Core.Projects;
using Keepi.Core.UserInvoiceItemCustomizations;
using Keepi.Core.Users;

namespace Keepi.Core.UserProjects;

public interface IGetUserProjects
{
    Task<IValueOrErrorResult<GetUserProjectResult, GetUserProjectsError>> Execute(
        UserId userId,
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
    ProjectId Id,
    ProjectName Name,
    bool Enabled,
    GetUserProjectResultProjectInvoiceItem[] InvoiceItems
);

public sealed record GetUserProjectResultProjectInvoiceItem(InvoiceItemId Id, InvoiceItemName Name);

public sealed record GetUserProjectResultInvoiceItemCustomization(
    InvoiceItemId InvoiceItemId,
    UserInvoiceITemCustomizationOrdinal Ordinal,
    Color? Color
);
