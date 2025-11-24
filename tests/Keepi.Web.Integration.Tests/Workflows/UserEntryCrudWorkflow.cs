using Keepi.Api.Projects.Update;
using Keepi.Api.UserEntries.GetWeek;
using Microsoft.VisualBasic;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UserEntryCrudWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Create_read_update_delete_entries_test()
    {
        var client = await applicationFactory.CreateClientWithRandomUser();

        var user = await client.GetUser();

        var firstProjectCreated = await client.CreateProject(
            new Api.Projects.Create.CreateProjectRequest
            {
                Name = Guid.NewGuid().ToString(),
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Dev", "Administratie"],
            }
        );
        var firstProject = await client.GetProject(projectId: firstProjectCreated.Id);
        var developmentUserInvoiceItemId = firstProject
            .InvoiceItems.Single(i => i.Name == "Dev")
            .Id;
        var administrationUserInvoiceItemId = firstProject
            .InvoiceItems.Single(i => i.Name == "Administratie")
            .Id;

        // Initial create
        await client.UpdateUserWeekEntries(
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
                            InvoiceItemId = developmentUserInvoiceItemId,
                            Minutes = 60,
                            Remark = "Nieuwe feature",
                        },
                        new()
                        {
                            InvoiceItemId = administrationUserInvoiceItemId,
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
                            InvoiceItemId = developmentUserInvoiceItemId,
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
                            InvoiceItemId = administrationUserInvoiceItemId,
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

        var savedWeek = await client.GetUserWeekEntries(year: 2025, weekNumber: 25);
        savedWeek.ShouldBeEquivalentTo(
            new GetWeekUserEntriesResponse(
                Monday: new(
                    Entries:
                    [
                        new(
                            InvoiceItemId: developmentUserInvoiceItemId,
                            Minutes: 60,
                            Remark: "Nieuwe feature"
                        ),
                        new(
                            InvoiceItemId: administrationUserInvoiceItemId,
                            Minutes: 45,
                            Remark: "Project Flyby"
                        ),
                    ]
                ),
                Tuesday: new(
                    Entries:
                    [
                        new(InvoiceItemId: developmentUserInvoiceItemId, Minutes: 30, Remark: null),
                    ]
                ),
                Wednesday: new(
                    Entries:
                    [
                        new(
                            InvoiceItemId: administrationUserInvoiceItemId,
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
        await client.UpdateUserWeekEntries(
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
                            InvoiceItemId = developmentUserInvoiceItemId,
                            Minutes = 61,
                            Remark = "Nieuwe feature1",
                        },
                        new()
                        {
                            InvoiceItemId = administrationUserInvoiceItemId,
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
                            InvoiceItemId = developmentUserInvoiceItemId,
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
                            InvoiceItemId = administrationUserInvoiceItemId,
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

        savedWeek = await client.GetUserWeekEntries(year: 2025, weekNumber: 25);
        savedWeek.ShouldBeEquivalentTo(
            new GetWeekUserEntriesResponse(
                Monday: new(
                    Entries:
                    [
                        new(
                            InvoiceItemId: developmentUserInvoiceItemId,
                            Minutes: 61,
                            Remark: "Nieuwe feature1"
                        ),
                        new(
                            InvoiceItemId: administrationUserInvoiceItemId,
                            Minutes: 46,
                            Remark: "Project Flyby1"
                        ),
                    ]
                ),
                Tuesday: new(
                    Entries:
                    [
                        new(InvoiceItemId: developmentUserInvoiceItemId, Minutes: 31, Remark: "1"),
                    ]
                ),
                Wednesday: new(
                    Entries:
                    [
                        new(
                            InvoiceItemId: administrationUserInvoiceItemId,
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
        await client.UpdateUserWeekEntries(
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
                            InvoiceItemId = developmentUserInvoiceItemId,
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
                            InvoiceItemId = developmentUserInvoiceItemId,
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
                            InvoiceItemId = administrationUserInvoiceItemId,
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

        savedWeek = await client.GetUserWeekEntries(year: 2025, weekNumber: 25);
        savedWeek.ShouldBeEquivalentTo(
            new GetWeekUserEntriesResponse(
                Monday: new(
                    Entries:
                    [
                        new(
                            InvoiceItemId: developmentUserInvoiceItemId,
                            Minutes: 61,
                            Remark: "Nieuwe feature1"
                        ),
                    ]
                ),
                Tuesday: new(
                    Entries:
                    [
                        new(InvoiceItemId: developmentUserInvoiceItemId, Minutes: 31, Remark: "1"),
                    ]
                ),
                Wednesday: new(
                    Entries:
                    [
                        new(
                            InvoiceItemId: administrationUserInvoiceItemId,
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

    [Fact]
    public async Task Disabled_project_entries_should_not_be_modified_by_week_entries_update()
    {
        var client = await applicationFactory.CreateClientWithRandomUser();

        var user = await client.GetUser();

        var firstProjectCreated = await client.CreateProject(
            new Api.Projects.Create.CreateProjectRequest
            {
                Name = Guid.NewGuid().ToString(),
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Dev"],
            }
        );
        var firstProject = await client.GetProject(projectId: firstProjectCreated.Id);
        var developmentUserInvoiceItemId = firstProject
            .InvoiceItems.Single(i => i.Name == "Dev")
            .Id;

        var secondProjectCreated = await client.CreateProject(
            new Api.Projects.Create.CreateProjectRequest
            {
                Name = Guid.NewGuid().ToString(),
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Administratie"],
            }
        );
        var secondProject = await client.GetProject(projectId: secondProjectCreated.Id);
        var administrationUserInvoiceItemId = secondProject
            .InvoiceItems.Single(i => i.Name == "Administratie")
            .Id;

        // Initial create
        await client.UpdateUserWeekEntries(
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
                            InvoiceItemId = developmentUserInvoiceItemId,
                            Minutes = 60,
                            Remark = "Nieuwe feature",
                        },
                        new()
                        {
                            InvoiceItemId = administrationUserInvoiceItemId,
                            Minutes = 45,
                            Remark = "Project Flyby",
                        },
                    ],
                },
                Tuesday = new() { Entries = [] },
                Wednesday = new() { Entries = [] },
                Thursday = new() { Entries = [] },
                Friday = new() { Entries = [] },
                Saturday = new() { Entries = [] },
                Sunday = new() { Entries = [] },
            }
        );

        var savedWeek = await client.GetUserWeekEntries(year: 2025, weekNumber: 25);
        savedWeek.ShouldBeEquivalentTo(
            new GetWeekUserEntriesResponse(
                Monday: new(
                    Entries:
                    [
                        new(
                            InvoiceItemId: developmentUserInvoiceItemId,
                            Minutes: 60,
                            Remark: "Nieuwe feature"
                        ),
                        new(
                            InvoiceItemId: administrationUserInvoiceItemId,
                            Minutes: 45,
                            Remark: "Project Flyby"
                        ),
                    ]
                ),
                Tuesday: new(Entries: []),
                Wednesday: new(Entries: []),
                Thursday: new(Entries: []),
                Friday: new(Entries: []),
                Saturday: new(Entries: []),
                Sunday: new(Entries: [])
            )
        );

        // Disable the second project
        await client.UpdateProject(
            projectId: secondProject.Id,
            new()
            {
                Name = secondProject.Name,
                Enabled = false,
                InvoiceItems = secondProject
                    .InvoiceItems.Select(i => new UpdateProjectRequestInvoiceItem
                    {
                        Id = i.Id,
                        Name = i.Name,
                    })
                    .ToArray(),
                UserIds = secondProject.Users.Select(u => (int?)u.Id).ToArray(),
            }
        );

        // Update "existing" entries
        await client.UpdateUserWeekEntries(
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
                            InvoiceItemId = developmentUserInvoiceItemId,
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
                            InvoiceItemId = developmentUserInvoiceItemId,
                            Minutes = 31,
                            Remark = "1",
                        },
                    ],
                },
                Wednesday = new() { Entries = [] },
                Thursday = new() { Entries = [] },
                Friday = new() { Entries = [] },
                Saturday = new() { Entries = [] },
                Sunday = new() { Entries = [] },
            }
        );

        savedWeek = await client.GetUserWeekEntries(year: 2025, weekNumber: 25);
        savedWeek.ShouldBeEquivalentTo(
            new GetWeekUserEntriesResponse(
                Monday: new(
                    Entries:
                    [
                        new(
                            InvoiceItemId: administrationUserInvoiceItemId,
                            Minutes: 45,
                            Remark: "Project Flyby"
                        ),
                        new(
                            InvoiceItemId: developmentUserInvoiceItemId,
                            Minutes: 61,
                            Remark: "Nieuwe feature1"
                        ),
                    ]
                ),
                Tuesday: new(
                    Entries:
                    [
                        new(InvoiceItemId: developmentUserInvoiceItemId, Minutes: 31, Remark: "1"),
                    ]
                ),
                Wednesday: new(Entries: []),
                Thursday: new(Entries: []),
                Friday: new(Entries: []),
                Saturday: new(Entries: []),
                Sunday: new(Entries: [])
            )
        );
    }
}
