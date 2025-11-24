namespace Keepi.Api.UserProjects.GetAll;

public record GetUserProjectsResponse(GetUserProjectsResponseProject[] Projects);

public record GetUserProjectsResponseProject(
    int Id,
    string Name,
    bool Enabled,
    GetUserProjectsResponseProjectInvoiceItem[] InvoiceItems
);

public record GetUserProjectsResponseProjectInvoiceItem(
    int Id,
    string Name,
    int Ordinal,
    string? Color // Example: '#FFAA11'
);
