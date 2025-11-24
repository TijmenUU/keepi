namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UserEntryExportWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Export_entries_test()
    {
        var client = await applicationFactory.CreateClientWithRandomUser();

        var user = await client.GetUser();

        var firstProjectCreated = await client.CreateProject(
            new Api.Projects.Create.CreateProjectRequest
            {
                Name = "UserEntryExportWorkflow",
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

        using var exportStream = new StreamReader(
            await client.GetUserEntriesExportStream(
                start: new DateOnly(2025, 6, 16),
                stop: new DateOnly(2025, 6, 22)
            )
        );
        exportStream.ReadLine().ShouldBe("Datum;Project;Post;Minuten;Opmerking");
        exportStream
            .ReadLine()
            .ShouldBe("16-06-2025;UserEntryExportWorkflow;Dev;60;Nieuwe feature");
        exportStream
            .ReadLine()
            .ShouldBe("16-06-2025;UserEntryExportWorkflow;Administratie;45;Project Flyby");
        exportStream.ReadLine().ShouldBe("17-06-2025;UserEntryExportWorkflow;Dev;30;");
        exportStream.ReadLine().ShouldBe("18-06-2025;UserEntryExportWorkflow;Administratie;15;");
        (await exportStream.ReadLineAsync()).ShouldBeNull();
    }
}
