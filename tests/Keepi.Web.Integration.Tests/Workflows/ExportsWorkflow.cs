namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UserEntryExportWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Export_user_entries_returns_expected_entries()
    {
        var adminClient = await applicationFactory.CreateClientForAdminUser();

        var firstUserClient = await applicationFactory.CreateClientForNormalUser(
            fullName: "EersteExportGebruiker",
            subjectClaim: Guid.NewGuid().ToString()
        );
        var firstUser = await firstUserClient.GetUser();

        var secondUserClient = await applicationFactory.CreateClientForNormalUser(
            fullName: "TweedeExportGebruiker",
            subjectClaim: Guid.NewGuid().ToString()
        );
        var secondUser = await secondUserClient.GetUser();

        var firstProjectCreated = await adminClient.CreateProject(
            new Api.Projects.Create.CreateProjectRequest
            {
                Name = "UserEntryExportWorkflow1",
                Enabled = true,
                UserIds = [firstUser.Id],
                InvoiceItemNames = ["Dev", "Administratie"],
            }
        );
        var secondProjectCreated = await adminClient.CreateProject(
            new Api.Projects.Create.CreateProjectRequest
            {
                Name = "UserEntryExportWorkflow2",
                Enabled = true,
                UserIds = [firstUser.Id, secondUser.Id],
                InvoiceItemNames = ["Overige"],
            }
        );

        var firstProject = await adminClient.GetProject(projectId: firstProjectCreated.Id);
        var secondProject = await adminClient.GetProject(projectId: secondProjectCreated.Id);

        var developmentUserInvoiceItemId = firstProject
            .InvoiceItems.Single(i => i.Name == "Dev")
            .Id;
        var administrationUserInvoiceItemId = firstProject
            .InvoiceItems.Single(i => i.Name == "Administratie")
            .Id;
        var miscellaneousUserInvoiceItemId = secondProject
            .InvoiceItems.Single(i => i.Name == "Overige")
            .Id;

        // Update week entries before export

        await firstUserClient.UpdateUserWeekEntries(
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
        await secondUserClient.UpdateUserWeekEntries(
            year: 2025,
            weekNumber: 25,
            request: new()
            {
                Monday = new() { Entries = [] },
                Tuesday = new() { Entries = [] },
                Wednesday = new()
                {
                    Entries =
                    [
                        new()
                        {
                            InvoiceItemId = miscellaneousUserInvoiceItemId,
                            Minutes = 75,
                            Remark = null,
                        },
                    ],
                },
                Thursday = new()
                {
                    Entries =
                    [
                        new()
                        {
                            InvoiceItemId = miscellaneousUserInvoiceItemId,
                            Minutes = 90,
                            Remark = null,
                        },
                    ],
                },
                Friday = new() { Entries = [] },
                Saturday = new() { Entries = [] },
                Sunday = new() { Entries = [] },
            }
        );

        // Validate the export

        using var exportStream = new StreamReader(
            await adminClient.GetUserEntriesExportStream(
                start: new DateOnly(2025, 6, 16),
                stop: new DateOnly(2025, 6, 22)
            )
        );
        exportStream.ReadLine().ShouldBe("Gebruiker;Datum;Project;Post;Minuten;Opmerking");

        var remainingLines = new List<string>();
        string? line = exportStream.ReadLine();
        while (line != null)
        {
            remainingLines.Add(line);
            line = exportStream.ReadLine();
        }

        // First user
        remainingLines.ShouldContain(
            "EersteExportGebruiker;16-06-2025;UserEntryExportWorkflow1;Dev;60;Nieuwe feature"
        );
        remainingLines.ShouldContain(
            "EersteExportGebruiker;16-06-2025;UserEntryExportWorkflow1;Administratie;45;Project Flyby"
        );
        remainingLines.ShouldContain(
            "EersteExportGebruiker;17-06-2025;UserEntryExportWorkflow1;Dev;30;"
        );
        remainingLines.ShouldContain(
            "EersteExportGebruiker;18-06-2025;UserEntryExportWorkflow1;Administratie;15;"
        );

        // Second user
        remainingLines.ShouldContain(
            "TweedeExportGebruiker;18-06-2025;UserEntryExportWorkflow2;Overige;75;"
        );
        remainingLines.ShouldContain(
            "TweedeExportGebruiker;19-06-2025;UserEntryExportWorkflow2;Overige;90;"
        );
    }
}
