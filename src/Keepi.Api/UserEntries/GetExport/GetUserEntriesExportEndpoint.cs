using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using FastEndpoints;
using Keepi.Api.Authorization;
using Keepi.Core.Entries;
using Microsoft.Extensions.Logging;

namespace Keepi.Api.UserEntries.GetExport;

public class GetUserEntriesExportEndpoint(
    IResolveUserHelper resolveUserHelper,
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
        var user = await resolveUserHelper.GetUserOrNull(
            userClaimsPrincipal: User,
            cancellationToken: cancellationToken
        );
        if (user == null)
        {
            logger.LogDebug("Refusing to export entries for unregistered user");
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

        if (result.TrySuccess(out var success, out var error))
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
                    records: success.Select(e => new ExportRecord(
                        Date: e.Date,
                        CategoryName: e.UserEntryCategoryName,
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

        if (error == ExportUserEntriesUseCaseError.StartGreaterThanStop)
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
        [property: Name("Categorie")] string CategoryName,
        [property: Name("Minuten")] int Minutes,
        [property: Name("Opmerking")] string Remark
    );
}
