namespace Keepi.Api.Projects.Update;

public sealed class UpdateProjectRequest
{
    public string? Name { get; set; }
    public bool? Enabled { get; set; }

    public int?[]? UserIds { get; set; }

    public UpdateProjectRequestInvoiceItem?[]? InvoiceItems { get; set; }
}

public sealed class UpdateProjectRequestInvoiceItem
{
    public int? Id { get; set; }
    public string? Name { get; set; }
}
