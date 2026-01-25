using Keepi.Api.Projects.GetAll;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class ProjectCrudWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Create_update_delete_test()
    {
        var adminClient = await applicationFactory.CreateClientForAdminUser();

        var firstUserClient = await applicationFactory.CreateClientForRandomNormalUser();
        var firstUser = await firstUserClient.GetUser();

        // Create

        var createdProject = await adminClient.CreateProject(
            new()
            {
                Name = "ProjectCrudWorkflow1",
                Enabled = true,
                UserIds = [firstUser.Id],
                InvoiceItemNames = ["Item1"],
            }
        );
        createdProject.Id.ShouldBeGreaterThan(0);

        var project = await adminClient.GetProject(projectId: createdProject.Id);
        project.Id.ShouldBe(createdProject.Id);
        project.Name.ShouldBe("ProjectCrudWorkflow1");
        project.Enabled.ShouldBe(true);
        project.Users.ShouldBeEquivalentTo(
            new[] { new GetAllProjectsResponseProjectUser(Id: firstUser.Id, Name: firstUser.Name) }
        );
        project.InvoiceItems.Length.ShouldBe(1);
        project.InvoiceItems[0].Name.ShouldBe("Item1");

        (await firstUserClient.GetUserProjects()).Projects.ShouldContain(p =>
            p.Id == createdProject.Id
        );

        // Update 1

        var createdInvoiceItem1Id = project.InvoiceItems[0].Id;

        var secondUserClient = await applicationFactory.CreateClientForRandomNormalUser();
        var secondUser = await secondUserClient.GetUser();

        await adminClient.UpdateProject(
            projectId: createdProject.Id,
            request: new()
            {
                Name = "ProjectCrudWorkflow11",
                Enabled = false,
                UserIds = [firstUser.Id, secondUser.Id],
                InvoiceItems =
                [
                    new() { Id = createdInvoiceItem1Id, Name = "Item11" },
                    new() { Id = null, Name = "Item2" },
                ],
            }
        );

        project = await adminClient.GetProject(projectId: createdProject.Id);
        project.Id.ShouldBe(createdProject.Id);
        project.Name.ShouldBe("ProjectCrudWorkflow11");
        project.Enabled.ShouldBeFalse();
        project.Users.ShouldBeEquivalentTo(
            new[]
            {
                new GetAllProjectsResponseProjectUser(Id: firstUser.Id, Name: firstUser.Name),
                new GetAllProjectsResponseProjectUser(Id: secondUser.Id, Name: secondUser.Name),
            }
        );
        project.InvoiceItems.Length.ShouldBe(2);
        project.InvoiceItems[0].Id.ShouldBe(createdInvoiceItem1Id);
        project.InvoiceItems[0].Name.ShouldBe("Item11");
        project.InvoiceItems[1].Name.ShouldBe("Item2");

        (await firstUserClient.GetUserProjects()).Projects.ShouldContain(p =>
            p.Id == createdProject.Id
        );
        (await secondUserClient.GetUserProjects()).Projects.ShouldContain(p =>
            p.Id == createdProject.Id
        );

        // Update 2

        var thirdUserclient = await applicationFactory.CreateClientForRandomNormalUser();
        var thirdUser = await thirdUserclient.GetUser();

        await adminClient.UpdateProject(
            projectId: createdProject.Id,
            request: new()
            {
                Name = "ProjectCrudWorkflow111",
                Enabled = true,
                UserIds = [thirdUser.Id],
                InvoiceItems = [new() { Id = null, Name = "Item3" }],
            }
        );

        project = await adminClient.GetProject(projectId: createdProject.Id);
        project.Id.ShouldBe(createdProject.Id);
        project.Name.ShouldBe("ProjectCrudWorkflow111");
        project.Enabled.ShouldBeTrue();
        project.Users.ShouldBeEquivalentTo(
            new[] { new GetAllProjectsResponseProjectUser(Id: thirdUser.Id, Name: thirdUser.Name) }
        );
        project.InvoiceItems.Length.ShouldBe(1);
        project.InvoiceItems[0].Name.ShouldBe("Item3");

        var firstUserProjects = await firstUserClient.GetUserProjects();
        firstUserProjects.Projects.ShouldBeEmpty();

        (await firstUserClient.GetUserProjects()).Projects.ShouldNotContain(p =>
            p.Id == createdProject.Id
        );
        (await secondUserClient.GetUserProjects()).Projects.ShouldNotContain(p =>
            p.Id == createdProject.Id
        );
        (await thirdUserclient.GetUserProjects()).Projects.ShouldContain(p =>
            p.Id == createdProject.Id
        );

        // Delete

        await adminClient.DeleteProject(projectId: createdProject.Id);

        var projects = await adminClient.GetAllProjects();
        projects.Projects.ShouldNotContain(p => p.Id == createdProject.Id);

        (await firstUserClient.GetUserProjects()).Projects.ShouldNotContain(p =>
            p.Id == createdProject.Id
        );
        (await secondUserClient.GetUserProjects()).Projects.ShouldNotContain(p =>
            p.Id == createdProject.Id
        );
        (await thirdUserclient.GetUserProjects()).Projects.ShouldNotContain(p =>
            p.Id == createdProject.Id
        );
    }

    [Fact]
    public async Task Delete_project_wipes_related_user_entries()
    {
        var adminClient = await applicationFactory.CreateClientForAdminUser();

        var userClient = await applicationFactory.CreateClientForRandomNormalUser();
        var user = await userClient.GetUser();

        // Create

        var createdProjectOne = await adminClient.CreateProject(
            new()
            {
                Name = "DeleteProjectWorkflow1",
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Item1"],
            }
        );
        createdProjectOne.Id.ShouldBeGreaterThan(0);

        var createdProjectTwo = await adminClient.CreateProject(
            new()
            {
                Name = "DeleteProjectWorkflow2",
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Item2"],
            }
        );
        createdProjectTwo.Id.ShouldBeGreaterThan(0);

        var projectOne = await adminClient.GetProject(projectId: createdProjectOne.Id);
        var projectTwo = await adminClient.GetProject(projectId: createdProjectTwo.Id);

        var projectOneInvoiceItemId = projectOne.InvoiceItems.First().Id;
        var projectTwoInvoiceItemId = projectTwo.InvoiceItems.First().Id;

        await userClient.UpdateUserWeekEntries(
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
                            InvoiceItemId = projectOneInvoiceItemId,
                            Minutes = 1,
                            Remark = null,
                        },
                        new()
                        {
                            InvoiceItemId = projectTwoInvoiceItemId,
                            Minutes = 2,
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

        var weekEntries = await userClient.GetUserWeekEntries(year: 2025, weekNumber: 25);
        weekEntries.Wednesday.Entries.ShouldContain(e =>
            e.Minutes == 1 && e.InvoiceItemId == projectOneInvoiceItemId
        );
        weekEntries.Wednesday.Entries.ShouldContain(e =>
            e.Minutes == 2 && e.InvoiceItemId == projectTwoInvoiceItemId
        );

        // Delete
        await adminClient.DeleteProject(projectId: projectOne.Id);

        weekEntries = await userClient.GetUserWeekEntries(year: 2025, weekNumber: 25);
        weekEntries.Wednesday.Entries.ShouldNotContain(e =>
            e.Minutes == 1 && e.InvoiceItemId == projectOneInvoiceItemId
        );
        weekEntries.Wednesday.Entries.ShouldContain(e =>
            e.Minutes == 2 && e.InvoiceItemId == projectTwoInvoiceItemId
        );
    }

    [Fact]
    public async Task Removing_user_from_project_wipes_related_user_entries()
    {
        var adminClient = await applicationFactory.CreateClientForAdminUser();

        var userClient = await applicationFactory.CreateClientForRandomNormalUser();
        var user = await userClient.GetUser();

        // Create

        var createdProjectOne = await adminClient.CreateProject(
            new()
            {
                Name = "DeleteProjectUserWorkflow1",
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Item1"],
            }
        );
        createdProjectOne.Id.ShouldBeGreaterThan(0);

        var createdProjectTwo = await adminClient.CreateProject(
            new()
            {
                Name = "DeleteProjectUserWorkflow2",
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Item2"],
            }
        );
        createdProjectTwo.Id.ShouldBeGreaterThan(0);

        var projectOne = await adminClient.GetProject(projectId: createdProjectOne.Id);
        var projectTwo = await adminClient.GetProject(projectId: createdProjectTwo.Id);

        var projectOneInvoiceItemId = projectOne.InvoiceItems.First().Id;
        var projectTwoInvoiceItemId = projectTwo.InvoiceItems.First().Id;

        await userClient.UpdateUserWeekEntries(
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
                            InvoiceItemId = projectOneInvoiceItemId,
                            Minutes = 1,
                            Remark = null,
                        },
                        new()
                        {
                            InvoiceItemId = projectTwoInvoiceItemId,
                            Minutes = 2,
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

        var weekEntries = await userClient.GetUserWeekEntries(year: 2025, weekNumber: 25);
        weekEntries.Wednesday.Entries.ShouldContain(e =>
            e.Minutes == 1 && e.InvoiceItemId == projectOneInvoiceItemId
        );
        weekEntries.Wednesday.Entries.ShouldContain(e =>
            e.Minutes == 2 && e.InvoiceItemId == projectTwoInvoiceItemId
        );

        // Remove user
        await adminClient.UpdateProject(
            projectId: projectOne.Id,
            request: new()
            {
                Name = projectOne.Name,
                Enabled = projectOne.Enabled,
                InvoiceItems =
                [
                    new()
                    {
                        Id = projectOneInvoiceItemId,
                        Name = projectOne
                            .InvoiceItems.Single(i => i.Id == projectOneInvoiceItemId)
                            .Name,
                    },
                ],
                UserIds = [],
            }
        );

        weekEntries = await userClient.GetUserWeekEntries(year: 2025, weekNumber: 25);
        weekEntries.Wednesday.Entries.ShouldNotContain(e =>
            e.Minutes == 1 && e.InvoiceItemId == projectOneInvoiceItemId
        );
        weekEntries.Wednesday.Entries.ShouldContain(e =>
            e.Minutes == 2 && e.InvoiceItemId == projectTwoInvoiceItemId
        );
    }

    [Fact]
    public async Task Delete_invoice_item_from_project_wipes_related_user_entries()
    {
        var adminClient = await applicationFactory.CreateClientForAdminUser();

        var userClient = await applicationFactory.CreateClientForRandomNormalUser();
        var user = await userClient.GetUser();

        // Create

        var createdProject = await adminClient.CreateProject(
            new()
            {
                Name = "DeleteInvoiceItemProjectWorkflow1",
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Item1", "Item2"],
            }
        );
        createdProject.Id.ShouldBeGreaterThan(0);
        var project = await adminClient.GetProject(projectId: createdProject.Id);
        var firstInvoiceItemId = project.InvoiceItems.First().Id;
        var secondInvoiceItemId = project.InvoiceItems.Skip(1).First().Id;

        await userClient.UpdateUserWeekEntries(
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
                            InvoiceItemId = firstInvoiceItemId,
                            Minutes = 1,
                            Remark = null,
                        },
                        new()
                        {
                            InvoiceItemId = secondInvoiceItemId,
                            Minutes = 2,
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

        var weekEntries = await userClient.GetUserWeekEntries(year: 2025, weekNumber: 25);
        weekEntries.Wednesday.Entries.ShouldContain(e =>
            e.Minutes == 1 && e.InvoiceItemId == firstInvoiceItemId
        );
        weekEntries.Wednesday.Entries.ShouldContain(e =>
            e.Minutes == 2 && e.InvoiceItemId == secondInvoiceItemId
        );

        // Delete invoice item 1
        await adminClient.UpdateProject(
            projectId: project.Id,
            request: new()
            {
                Name = project.Name,
                Enabled = project.Enabled,
                InvoiceItems =
                [
                    new()
                    {
                        Id = secondInvoiceItemId,
                        Name = project.InvoiceItems.Single(i => i.Id == secondInvoiceItemId).Name,
                    },
                ],
                UserIds = [user.Id],
            }
        );

        weekEntries = await userClient.GetUserWeekEntries(year: 2025, weekNumber: 25);
        weekEntries.Wednesday.Entries.ShouldNotContain(e =>
            e.Minutes == 1 && e.InvoiceItemId == firstInvoiceItemId
        );
        weekEntries.Wednesday.Entries.ShouldContain(e =>
            e.Minutes == 2 && e.InvoiceItemId == secondInvoiceItemId
        );
    }
}
