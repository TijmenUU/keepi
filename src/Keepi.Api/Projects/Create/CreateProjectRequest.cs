namespace Keepi.Api.Projects.Create;

public sealed class CreateProjectRequest
{
    public string? Name { get; set; }
    public bool? Enabled { get; set; }

    public int?[]? UserIds { get; set; }

    public string?[]? InvoiceItemNames { get; set; }
}
