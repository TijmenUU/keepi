namespace Keepi.Api.Exports.GetUserEntriesExport;

public sealed class GetUserEntriesExportEndpointRequest
{
    public DateOnly? Start { get; set; }
    public DateOnly? Stop { get; set; }
}
