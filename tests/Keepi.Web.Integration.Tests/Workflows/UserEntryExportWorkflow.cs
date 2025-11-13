using Keepi.Api.UserEntries.GetExport;
using Keepi.Api.UserEntries.UpdateWeek;
using Keepi.Api.UserEntryCategories.GetAll;
using Keepi.Api.UserEntryCategories.UpdateAll;
using Microsoft.VisualBasic;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UserEntryExportWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Export_entries_test()
    {
        var client = await applicationFactory.CreateRegisteredUserClient(
            userName: "Ton de Maks",
            userSubjectClaim: Guid.NewGuid().ToString()
        );

        var createdCategoryIds = await CreateUserEntryCategories(
            client: client,
            values: [(name: "Dev", ordinal: 1), (name: "Administratie", ordinal: 2)]
        );
        var developmentUserEntryCategoryId = createdCategoryIds[0];
        var administrationUserEntryCategoryId = createdCategoryIds[1];

        // Initial create
        await UpdateWeekEntries(
            client: client,
            year: 2025,
            weekNumber: 25,
            request: new()
            {
                Monday = new()
                {
                    Entries =
                    [
                        new()
                        {
                            EntryCategoryId = developmentUserEntryCategoryId,
                            Minutes = 60,
                            Remark = "Nieuwe feature",
                        },
                        new()
                        {
                            EntryCategoryId = administrationUserEntryCategoryId,
                            Minutes = 45,
                            Remark = "Project Flyby",
                        },
                    ],
                },
                Tuesday = new()
                {
                    Entries =
                    [
                        new()
                        {
                            EntryCategoryId = developmentUserEntryCategoryId,
                            Minutes = 30,
                            Remark = null,
                        },
                    ],
                },
                Wednesday = new()
                {
                    Entries =
                    [
                        new()
                        {
                            EntryCategoryId = administrationUserEntryCategoryId,
                            Minutes = 15,
                            Remark = null,
                        },
                    ],
                },
                Thursday = new() { Entries = [] },
                Friday = new() { Entries = [] },
                Saturday = new() { Entries = [] },
                Sunday = new() { Entries = [] },
            }
        );

        using var exportStream = new StreamReader(
            await GetExportStream(
                client: client,
                start: new DateOnly(2025, 6, 16),
                stop: new DateOnly(2025, 6, 22)
            )
        );
        exportStream.ReadLine().ShouldBe("Datum;Categorie;Minuten;Opmerking");
        exportStream.ReadLine().ShouldBe("16-06-2025;Dev;60;Nieuwe feature");
        exportStream.ReadLine().ShouldBe("16-06-2025;Administratie;45;Project Flyby");
        exportStream.ReadLine().ShouldBe("17-06-2025;Dev;30;");
        exportStream.ReadLine().ShouldBe("18-06-2025;Administratie;15;");
        (await exportStream.ReadLineAsync()).ShouldBeNull();
    }

    private static async Task<int[]> CreateUserEntryCategories(
        HttpClient client,
        (string name, int ordinal)[] values
    )
    {
        var httpResponse = await client.PutAsJsonAsync(
            requestUri: "/api/user/entrycategories",
            value: new PutUpdateUserEntryCategoriesRequest
            {
                UserEntryCategories = values
                    .Select(v => new PutUpdateUserEntryCategoriesRequestCategory()
                    {
                        Name = v.name,
                        Ordinal = v.ordinal,
                        ActiveFrom = null,
                        ActiveTo = null,
                        Enabled = true,
                    })
                    .ToArray(),
            },
            options: KeepiWebApplicationFactory.GetDefaultJsonSerializerOptions()
        );
        httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

        var userEntryCategories = await client.GetFromJsonAsync<GetUserUserEntryCategoriesResponse>(
            requestUri: "/api/user/entrycategories",
            options: KeepiWebApplicationFactory.GetDefaultJsonSerializerOptions()
        );
        userEntryCategories.ShouldNotBeNull();

        var ids = new List<int>();
        foreach (var value in values)
        {
            userEntryCategories.Categories.ShouldContain(c => c.Name == value.name);
            ids.Add(userEntryCategories.Categories.Single(c => c.Name == value.name).Id);
        }

        return ids.ToArray();
    }

    private static async Task UpdateWeekEntries(
        HttpClient client,
        int year,
        int weekNumber,
        PutUpdateWeekUserEntriesRequest request
    )
    {
        var httpResponse = await client.PutAsJsonAsync(
            requestUri: $"/api/user/entries/year/{year}/week/{weekNumber}",
            value: request,
            options: KeepiWebApplicationFactory.GetDefaultJsonSerializerOptions()
        );
        httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        httpResponse.Headers.Location?.OriginalString.ShouldBe(
            $"/api/user/entries/year/{year}/week/{weekNumber}"
        );
    }

    private static async Task<Stream> GetExportStream(
        HttpClient client,
        DateOnly start,
        DateOnly stop
    )
    {
        var httpResponse = await client.PostAsJsonAsync(
            requestUri: "/api/user/entries/export",
            value: new GetUserEntriesExportEndpointRequest { Start = start, Stop = stop },
            options: KeepiWebApplicationFactory.GetDefaultJsonSerializerOptions()
        );
        httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        return await httpResponse.Content.ReadAsStreamAsync();
    }
}
