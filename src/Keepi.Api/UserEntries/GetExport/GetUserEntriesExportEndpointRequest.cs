namespace Keepi.Api.UserEntries.GetExport;

public class GetUserEntriesExportEndpointRequest
{
    public DateOnly? Start { get; set; }
    public DateOnly? Stop { get; set; }
}
