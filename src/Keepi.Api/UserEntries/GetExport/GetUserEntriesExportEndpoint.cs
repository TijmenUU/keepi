using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using FastEndpoints;
using Keepi.Core.Entries;
using Keepi.Core.Users;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntries.GetExport;

public sealed class GetUserEntriesExportEndpoint(
    IResolveUser resolveUser,
    IExportUserEntriesUseCase exportUserEntriesUseCase,
    ILogger<GetUserEntriesExportEndpoint> logger
) : Endpoint<GetUserEntriesExportEndpointRequest>
{
    public override void Configure()
    {
        Post("/user/entries/export");
    }

    public override async Task HandleAsync(
        GetUserEntriesExportEndpointRequest request,
        CancellationToken cancellationToken
    )
    {
        var resolveUserResult = await resolveUser.Execute(cancellationToken: cancellationToken);
        if (!resolveUserResult.TrySuccess(out var user, out _))
        {
            logger.LogDebug("Refusing to export entries for unknown user");
            await Send.ForbiddenAsync(cancellation: cancellationToken);
            return;
        }

        if (!TryGetValidatedModel(request: request, out var validatedRequest))
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        var result = exportUserEntriesUseCase.Execute(
            userId: user.Id,
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

        if (errorResult == ExportUserEntriesUseCaseError.StartGreaterThanStop)
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        await Send.ErrorsAsync(statusCode: 500, cancellation: cancellationToken);
        return;
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

    record ExportRecord(
        [property: Name("Datum")] DateOnly Date,
        [property: Name("Project")] string ProjectName,
        [property: Name("Post")] string InvoiceItemName,
        [property: Name("Minuten")] int Minutes,
        [property: Name("Opmerking")] string Remark
    );
}
