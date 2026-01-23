using Keepi.Api.Projects.GetAll;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class ProjectCrudWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Create_update_delete_test()
    {
        var adminClient = await applicationFactory.CreateClientForAdminUser();

        var firstUser = await (
            await applicationFactory.CreateClientForRandomNormalUser()
        ).GetUser();

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

        // Update 1

        var createdInvoiceItem1Id = project.InvoiceItems[0].Id;

        var secondUser = await (
            await applicationFactory.CreateClientForRandomNormalUser()
        ).GetUser();

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

        // Update 2

        var thirdUser = await (
            await applicationFactory.CreateClientForRandomNormalUser()
        ).GetUser();

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

        // Delete

        await adminClient.DeleteProject(projectId: createdProject.Id);

        var projects = await adminClient.GetAllProjects();
        projects.Projects.ShouldNotContain(p => p.Id == createdProject.Id);
    }
}
