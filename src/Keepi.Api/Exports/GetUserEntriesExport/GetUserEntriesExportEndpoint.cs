using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using FastEndpoints;
using Keepi.Core.Exports;

namespace Keepi.Api.Exports.GetUserEntriesExport;

public sealed class GetUserEntriesExportEndpoint(IExportUserEntriesUseCase exportUserEntriesUseCase)
    : Endpoint<GetUserEntriesExportEndpointRequest>
{
    public override void Configure()
    {
        Post("/export/userentries");
    }

    public override async Task HandleAsync(
        GetUserEntriesExportEndpointRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!TryGetValidatedModel(request: request, out var validatedRequest))
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var result = await exportUserEntriesUseCase.Execute(
            start: validatedRequest.Start,
            stop: validatedRequest.Stop,
            cancellationToken: cancellationToken
        );

        if (result.TrySuccess(out var successResult, out var errorResult))
        {
            var temporaryFilePath = Path.GetTempFileName();
            {
                using var outputStream = new CsvWriter(
                    new StreamWriter(
                        new FileStream(path: temporaryFilePath, mode: FileMode.Append)
                    ),
                    CultureInfo.GetCultureInfo("nl-NL")
                );
                await outputStream.WriteRecordsAsync(
                    records: successResult.Select(e => new ExportRecord(
                        UserName: e.UserName,
                        Date: e.Date,
                        ProjectName: e.ProjectName,
                        InvoiceItemName: e.InvoiceItemName,
                        Minutes: e.Minutes,
                        Remark: e.Remark
                    )),
                    cancellationToken: cancellationToken
                );
            }

            await Send.StreamAsync(
                stream: File.OpenRead(temporaryFilePath),
                fileName: $"export_{validatedRequest.Start:yyyy-MM-dd}_{validatedRequest.Stop:yyyy-MM-dd}.csv",
                contentType: "text/csv",
                cancellation: cancellationToken
            );
            return;
        }

        await (
            errorResult switch
            {
                ExportUserEntriesUseCaseError.UnauthenticatedUser => Send.UnauthorizedAsync(
                    cancellation: cancellationToken
                ),
                ExportUserEntriesUseCaseError.UnauthorizedUser => Send.ForbiddenAsync(
                    cancellation: cancellationToken
                ),
                ExportUserEntriesUseCaseError.Unknown => Send.ErrorsAsync(
                    statusCode: 500,
                    cancellation: cancellationToken
                ),
                _ => Send.ErrorsAsync(cancellation: cancellationToken),
            }
        );
    }

    private static bool TryGetValidatedModel(
        GetUserEntriesExportEndpointRequest request,
        [NotNullWhen(returnValue: true)] out ValidatedGetUserEntriesExportEndpointRequest? validated
    )
    {
        if (
            request == null
            || request.Start == null
            || request.Stop == null
            || request.Start > request.Stop
        )
        {
            validated = null;
            return false;
        }

        validated = new ValidatedGetUserEntriesExportEndpointRequest(
            Start: request.Start.Value,
            Stop: request.Stop.Value
        );
        return true;
    }

    record ValidatedGetUserEntriesExportEndpointRequest(DateOnly Start, DateOnly Stop);

    // TODO The name of user could not be enough to identify them, maybe export the email address or user ID as well?
    record ExportRecord(
        [property: Name("Gebruiker")] string UserName,
        [property: Name("Datum")] DateOnly Date,
        [property: Name("Project")] string ProjectName,
        [property: Name("Post")] string InvoiceItemName,
        [property: Name("Minuten")] int Minutes,
        [property: Name("Opmerking")] string Remark
    );
}
