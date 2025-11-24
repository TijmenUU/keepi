namespace Keepi.Api.UserEntries.GetExport;

public sealed class GetUserEntriesExportEndpointRequest
{
    public DateOnly? Start { get; set; }
    public DateOnly? Stop { get; set; }
}
