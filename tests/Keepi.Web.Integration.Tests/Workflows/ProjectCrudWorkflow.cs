using Keepi.Api.Projects.GetAll;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class ProjectCrudWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Create_update_delete_test()
    {
        var client = await applicationFactory.CreateClientWithRandomUser();

        var user = await client.GetUser();

        // Create

        var createdProject = await client.CreateProject(
            new()
            {
                Name = "ProjectCrudWorkflow1",
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Item1"],
            }
        );
        createdProject.Id.ShouldBeGreaterThan(0);

        var project = await client.GetProject(projectId: createdProject.Id);
        project.Id.ShouldBe(createdProject.Id);
        project.Name.ShouldBe("ProjectCrudWorkflow1");
        project.Enabled.ShouldBe(true);
        project.Users.ShouldBeEquivalentTo(
            new[] { new GetAllProjectsResponseProjectUser(Id: user.Id, Name: user.Name) }
        );
        project.InvoiceItems.Length.ShouldBe(1);
        project.InvoiceItems[0].Name.ShouldBe("Item1");

        // Update 1

        var createdInvoiceItem1Id = project.InvoiceItems[0].Id;

        var otherUser1 = await (await applicationFactory.CreateClientWithRandomUser()).GetUser();

        await client.UpdateProject(
            projectId: createdProject.Id,
            request: new()
            {
                Name = "ProjectCrudWorkflow11",
                Enabled = false,
                UserIds = [user.Id, otherUser1.Id],
                InvoiceItems =
                [
                    new() { Id = createdInvoiceItem1Id, Name = "Item11" },
                    new() { Id = null, Name = "Item2" },
                ],
            }
        );

        project = await client.GetProject(projectId: createdProject.Id);
        project.Id.ShouldBe(createdProject.Id);
        project.Name.ShouldBe("ProjectCrudWorkflow11");
        project.Enabled.ShouldBeFalse();
        project.Users.ShouldBeEquivalentTo(
            new[]
            {
                new GetAllProjectsResponseProjectUser(Id: user.Id, Name: user.Name),
                new GetAllProjectsResponseProjectUser(Id: otherUser1.Id, Name: otherUser1.Name),
            }
        );
        project.InvoiceItems.Length.ShouldBe(2);
        project.InvoiceItems[0].Id.ShouldBe(createdInvoiceItem1Id);
        project.InvoiceItems[0].Name.ShouldBe("Item11");
        project.InvoiceItems[1].Name.ShouldBe("Item2");

        // Update 2

        var otherUser2 = await (await applicationFactory.CreateClientWithRandomUser()).GetUser();

        await client.UpdateProject(
            projectId: createdProject.Id,
            request: new()
            {
                Name = "ProjectCrudWorkflow111",
                Enabled = true,
                UserIds = [otherUser2.Id],
                InvoiceItems = [new() { Id = null, Name = "Item3" }],
            }
        );

        project = await client.GetProject(projectId: createdProject.Id);
        project.Id.ShouldBe(createdProject.Id);
        project.Name.ShouldBe("ProjectCrudWorkflow111");
        project.Enabled.ShouldBeTrue();
        project.Users.ShouldBeEquivalentTo(
            new[]
            {
                new GetAllProjectsResponseProjectUser(Id: otherUser2.Id, Name: otherUser2.Name),
            }
        );
        project.InvoiceItems.Length.ShouldBe(1);
        project.InvoiceItems[0].Name.ShouldBe("Item3");

        // Delete

        await client.DeleteProject(projectId: createdProject.Id);

        var projects = await client.GetAllProjects();
        projects.Projects.ShouldNotContain(p => p.Id == createdProject.Id);
    }
}
