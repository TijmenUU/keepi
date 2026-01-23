namespace Keepi.Web.Integration.Tests;

[Collection(DefaultCollection.Name)]
public class AuthorizationTests(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Create_project_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        var user = await (await applicationFactory.CreateClientForRandomNormalUser()).GetUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(() =>
            client.CreateProject(
                request: new()
                {
                    Name = "AuthorizationTest",
                    Enabled = true,
                    UserIds = [user.Id],
                    InvoiceItemNames = ["Item1"],
                }
            )
        );
    }

    [Fact]
    public async Task Delete_project_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(() =>
            client.DeleteProject(projectId: 99)
        );
    }

    [Fact]
    public async Task Get_all_projects_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(client.GetAllProjects);
    }

    [Fact]
    public async Task Update_project_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        var user = await (await applicationFactory.CreateClientForRandomNormalUser()).GetUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(() =>
            client.UpdateProject(
                projectId: 99,
                request: new()
                {
                    Name = "AuthorizationTest",
                    Enabled = true,
                    UserIds = [user.Id],
                    InvoiceItems = [new() { Id = null, Name = "Item1" }],
                }
            )
        );
    }

    [Fact]
    public async Task Get_user_entries_export_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(
            client.GetUserEntriesExportStream(
                start: new DateOnly(year: 2025, month: 12, day: 15),
                stop: new DateOnly(year: 2025, month: 12, day: 29)
            )
        );
    }

    [Fact]
    public async Task Get_user_entries_week_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(
            client.GetUserWeekEntries(year: 2025, weekNumber: 23)
        );
    }

    [Fact]
    public async Task Update_user_entries_week_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(
            client.UpdateUserWeekEntries(
                year: 2025,
                weekNumber: 25,
                request: new()
                {
                    Monday = new() { Entries = [] },
                    Tuesday = new() { Entries = [] },
                    Wednesday = new() { Entries = [] },
                    Thursday = new() { Entries = [] },
                    Friday = new() { Entries = [] },
                    Saturday = new() { Entries = [] },
                    Sunday = new() { Entries = [] },
                }
            )
        );
    }

    [Fact]
    public async Task Update_invoice_item_customizations_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(
            client.UpdateUserInvoiceItemCustomizations(request: new() { InvoiceItems = [] })
        );
    }

    [Fact]
    public async Task Get_user_projects_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(client.GetUserProjects);
    }

    [Fact]
    public async Task Get_all_users_endpoint_returns_forbidden()
    {
        var client = await applicationFactory.CreateClientForRandomZeroPermissionsUser();
        await Should.ThrowAsync<KeepiClientForbiddenException>(client.GetAllUsers);
    }
}
