using Keepi.Api.UserEntries.GetWeek;
using Keepi.Api.UserEntries.UpdateWeek;
using Keepi.Api.UserEntryCategories.Create;
using Microsoft.VisualBasic;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UserEntryCrudWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Create_read_update_delete_entries_test()
    {
        var client = await applicationFactory.CreateRegisteredUserClient(
            userName: "Paul de Groot",
            userSubjectClaim: Guid.NewGuid().ToString()
        );

        var developmentUserEntryCategoryId = await CreateUserEntryCategory(
            client: client,
            name: "Dev",
            ordinal: 1
        );
        var administrationUserEntryCategoryId = await CreateUserEntryCategory(
            client: client,
            name: "Administratie",
            ordinal: 2
        );

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

        var savedWeek = await GetWeekEntries(client: client, year: 2025, weekNumber: 25);
        savedWeek.ShouldBeEquivalentTo(
            new GetWeekUserEntriesResponse(
                Monday: new(
                    Entries:
                    [
                        new(
                            EntryCategoryId: developmentUserEntryCategoryId,
                            Minutes: 60,
                            Remark: "Nieuwe feature"
                        ),
                        new(
                            EntryCategoryId: administrationUserEntryCategoryId,
                            Minutes: 45,
                            Remark: "Project Flyby"
                        ),
                    ]
                ),
                Tuesday: new(
                    Entries:
                    [
                        new(
                            EntryCategoryId: developmentUserEntryCategoryId,
                            Minutes: 30,
                            Remark: null
                        ),
                    ]
                ),
                Wednesday: new(
                    Entries:
                    [
                        new(
                            EntryCategoryId: administrationUserEntryCategoryId,
                            Minutes: 15,
                            Remark: null
                        ),
                    ]
                ),
                Thursday: new(Entries: []),
                Friday: new(Entries: []),
                Saturday: new(Entries: []),
                Sunday: new(Entries: [])
            )
        );

        // Update "existing" entries
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
                            Minutes = 61,
                            Remark = "Nieuwe feature1",
                        },
                        new()
                        {
                            EntryCategoryId = administrationUserEntryCategoryId,
                            Minutes = 46,
                            Remark = "Project Flyby1",
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
                            Minutes = 31,
                            Remark = "1",
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
                            Minutes = 16,
                            Remark = "2",
                        },
                    ],
                },
                Thursday = new() { Entries = [] },
                Friday = new() { Entries = [] },
                Saturday = new() { Entries = [] },
                Sunday = new() { Entries = [] },
            }
        );

        savedWeek = await GetWeekEntries(client: client, year: 2025, weekNumber: 25);
        savedWeek.ShouldBeEquivalentTo(
            new GetWeekUserEntriesResponse(
                Monday: new(
                    Entries:
                    [
                        new(
                            EntryCategoryId: developmentUserEntryCategoryId,
                            Minutes: 61,
                            Remark: "Nieuwe feature1"
                        ),
                        new(
                            EntryCategoryId: administrationUserEntryCategoryId,
                            Minutes: 46,
                            Remark: "Project Flyby1"
                        ),
                    ]
                ),
                Tuesday: new(
                    Entries:
                    [
                        new(
                            EntryCategoryId: developmentUserEntryCategoryId,
                            Minutes: 31,
                            Remark: "1"
                        ),
                    ]
                ),
                Wednesday: new(
                    Entries:
                    [
                        new(
                            EntryCategoryId: administrationUserEntryCategoryId,
                            Minutes: 16,
                            Remark: "2"
                        ),
                    ]
                ),
                Thursday: new(Entries: []),
                Friday: new(Entries: []),
                Saturday: new(Entries: []),
                Sunday: new(Entries: [])
            )
        );

        // Deleting a single entry
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
                            Minutes = 61,
                            Remark = "Nieuwe feature1",
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
                            Minutes = 31,
                            Remark = "1",
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
                            Minutes = 16,
                            Remark = "2",
                        },
                    ],
                },
                Thursday = new() { Entries = [] },
                Friday = new() { Entries = [] },
                Saturday = new() { Entries = [] },
                Sunday = new() { Entries = [] },
            }
        );

        savedWeek = await GetWeekEntries(client: client, year: 2025, weekNumber: 25);
        savedWeek.ShouldBeEquivalentTo(
            new GetWeekUserEntriesResponse(
                Monday: new(
                    Entries:
                    [
                        new(
                            EntryCategoryId: developmentUserEntryCategoryId,
                            Minutes: 61,
                            Remark: "Nieuwe feature1"
                        ),
                    ]
                ),
                Tuesday: new(
                    Entries:
                    [
                        new(
                            EntryCategoryId: developmentUserEntryCategoryId,
                            Minutes: 31,
                            Remark: "1"
                        ),
                    ]
                ),
                Wednesday: new(
                    Entries:
                    [
                        new(
                            EntryCategoryId: administrationUserEntryCategoryId,
                            Minutes: 16,
                            Remark: "2"
                        ),
                    ]
                ),
                Thursday: new(Entries: []),
                Friday: new(Entries: []),
                Saturday: new(Entries: []),
                Sunday: new(Entries: [])
            )
        );
    }

    private static async Task<int> CreateUserEntryCategory(
        HttpClient client,
        string name,
        int ordinal
    )
    {
        var httpResponse = await client.PostAsJsonAsync(
            requestUri: "/api/user/entrycategories",
            value: new PostCreateUserUserEntryCategoryRequest
            {
                Name = name,
                Ordinal = ordinal,
                ActiveFrom = null,
                ActiveTo = null,
                Enabled = true,
            }
        );
        httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        httpResponse.Headers.Location?.OriginalString.ShouldBe("/api/user/entrycategories");

        var postCreateUserEntryCategoryResponse =
            await httpResponse.Content.ReadFromJsonAsync<PostCreateUserUserEntryCategoryResponse>();
        postCreateUserEntryCategoryResponse.ShouldNotBeNull();

        return postCreateUserEntryCategoryResponse.Id;
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
            value: request
        );
        httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        httpResponse.Headers.Location?.OriginalString.ShouldBe(
            $"/api/user/entries/year/{year}/week/{weekNumber}"
        );
    }

    private static async Task<GetWeekUserEntriesResponse> GetWeekEntries(
        HttpClient client,
        int year,
        int weekNumber
    )
    {
        var httpResponse = await client.GetAsync(
            requestUri: $"/api/user/entries/year/{year}/week/{weekNumber}"
        );
        httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        var model = await httpResponse.Content.ReadFromJsonAsync<GetWeekUserEntriesResponse>();
        model.ShouldNotBeNull();

        return model;
    }
}
