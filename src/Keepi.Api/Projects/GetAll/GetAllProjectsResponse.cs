namespace Keepi.Api.Projects.GetAll;

public record GetAllProjectsResponse(GetAllProjectsResponseProject[] Projects);

public record GetAllProjectsResponseProject(
    int Id,
    string Name,
    bool Enabled,
    GetAllProjectsResponseProjectUser[] Users,
    GetAllProjectsResponseProjectInvoiceItem[] InvoiceItems
);

public record GetAllProjectsResponseProjectUser(int Id, string Name);

public record GetAllProjectsResponseProjectInvoiceItem(int Id, string Name);
