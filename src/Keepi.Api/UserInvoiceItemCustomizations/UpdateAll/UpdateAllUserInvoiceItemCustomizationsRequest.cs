namespace Keepi.Api.UserInvoiceItemCustomizations.UpdateAll;

public sealed class UpdateAllUserInvoiceItemCustomizationsRequest
{
    public UpdateAllUserInvoiceItemCustomizationsRequestInvoiceItem?[]? InvoiceItems { get; set; }
}

public sealed class UpdateAllUserInvoiceItemCustomizationsRequestInvoiceItem
{
    public int? Id { get; set; }
    public int? Ordinal { get; set; }

    // Expected format: '#FFAA11'
    public string? Color { get; set; }
}
