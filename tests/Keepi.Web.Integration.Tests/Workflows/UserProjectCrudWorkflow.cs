using Keepi.Api.UserProjects.GetAll;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UserProjectCrudWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    private const string RedHexColorString = "#ff0000";
    private const string GreenHexColorString = "#00ff00";
    private const string BlueHexColorString = "#0000ff";

    [Fact]
    public async Task Create_update_delete_test()
    {
        var adminClient = await applicationFactory.CreateClientForAdminUser();
        var userClient = await applicationFactory.CreateClientForRandomNormalUser();

        var user = await userClient.GetUser();

        var createdProjectOne = await adminClient.CreateProject(
            new()
            {
                Name = "UserProjectCrudWorkflow1",
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Item1", "Item2", "Item3"],
            }
        );
        createdProjectOne.Id.ShouldBeGreaterThan(0);

        var createdProjectTwo = await adminClient.CreateProject(
            new()
            {
                Name = "UserProjectCrudWorkflow2",
                Enabled = true,
                UserIds = [user.Id],
                InvoiceItemNames = ["Item4", "Item5", "Item6"],
            }
        );
        createdProjectTwo.Id.ShouldBeGreaterThan(0);

        // Initial sanity check

        var allProjects = await adminClient.GetAllProjects();
        allProjects.Projects.ShouldContain(p => p.Id == createdProjectOne.Id);
        allProjects.Projects.ShouldContain(p => p.Id == createdProjectTwo.Id);

        var userProjects = await userClient.GetUserProjects();
        userProjects.Projects.Length.ShouldBe(2);

        userProjects.Projects[0].Id.ShouldBe(createdProjectOne.Id);
        userProjects.Projects[0].InvoiceItems.Length.ShouldBe(3);
        userProjects.Projects[0].InvoiceItems[0].Name.ShouldBe("Item1");
        userProjects.Projects[0].InvoiceItems[1].Name.ShouldBe("Item2");
        userProjects.Projects[0].InvoiceItems[2].Name.ShouldBe("Item3");

        userProjects.Projects[0].InvoiceItems.ShouldAllBe(i => i.Ordinal == int.MaxValue);
        userProjects.Projects[0].InvoiceItems.ShouldAllBe(i => i.Color == null);

        userProjects.Projects[1].Id.ShouldBe(createdProjectTwo.Id);
        userProjects.Projects[1].InvoiceItems.Length.ShouldBe(3);
        userProjects.Projects[1].InvoiceItems[0].Name.ShouldBe("Item4");
        userProjects.Projects[1].InvoiceItems[1].Name.ShouldBe("Item5");
        userProjects.Projects[1].InvoiceItems[2].Name.ShouldBe("Item6");

        userProjects.Projects[1].InvoiceItems.ShouldAllBe(i => i.Ordinal == int.MaxValue);
        userProjects.Projects[1].InvoiceItems.ShouldAllBe(i => i.Color == null);

        var invoiceItem1Id = userProjects.Projects[0].InvoiceItems[0].Id;
        var invoiceItem2Id = userProjects.Projects[0].InvoiceItems[1].Id;
        var invoiceItem3Id = userProjects.Projects[0].InvoiceItems[2].Id;
        var invoiceItem4Id = userProjects.Projects[1].InvoiceItems[0].Id;
        var invoiceItem5Id = userProjects.Projects[1].InvoiceItems[1].Id;
        var invoiceItem6Id = userProjects.Projects[1].InvoiceItems[2].Id;

        // Create

        await userClient.UpdateUserInvoiceItemCustomizations(
            request: new()
            {
                InvoiceItems =
                [
                    new()
                    {
                        Id = invoiceItem1Id,
                        Ordinal = 6,
                        Color = RedHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem2Id,
                        Ordinal = 5,
                        Color = RedHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem3Id,
                        Ordinal = 4,
                        Color = RedHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem4Id,
                        Ordinal = 3,
                        Color = RedHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem5Id,
                        Ordinal = 2,
                        Color = RedHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem6Id,
                        Ordinal = 1,
                        Color = RedHexColorString,
                    },
                ],
            }
        );

        userProjects = await userClient.GetUserProjects();
        userProjects.ShouldBeEquivalentTo(
            new GetUserProjectsResponse(
                Projects:
                [
                    new(
                        Id: createdProjectOne.Id,
                        Name: "UserProjectCrudWorkflow1",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: invoiceItem3Id,
                                Name: "Item3",
                                Ordinal: 4,
                                Color: RedHexColorString
                            ),
                            new(
                                Id: invoiceItem2Id,
                                Name: "Item2",
                                Ordinal: 5,
                                Color: RedHexColorString
                            ),
                            new(
                                Id: invoiceItem1Id,
                                Name: "Item1",
                                Ordinal: 6,
                                Color: RedHexColorString
                            ),
                        ]
                    ),
                    new(
                        Id: createdProjectTwo.Id,
                        Name: "UserProjectCrudWorkflow2",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: invoiceItem6Id,
                                Name: "Item6",
                                Ordinal: 1,
                                Color: RedHexColorString
                            ),
                            new(
                                Id: invoiceItem5Id,
                                Name: "Item5",
                                Ordinal: 2,
                                Color: RedHexColorString
                            ),
                            new(
                                Id: invoiceItem4Id,
                                Name: "Item4",
                                Ordinal: 3,
                                Color: RedHexColorString
                            ),
                        ]
                    ),
                ]
            )
        );

        // Update 1: partial update of 1 project

        await userClient.UpdateUserInvoiceItemCustomizations(
            request: new()
            {
                InvoiceItems =
                [
                    new()
                    {
                        Id = invoiceItem1Id,
                        Ordinal = 1,
                        Color = RedHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem2Id,
                        Ordinal = 2,
                        Color = GreenHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem3Id,
                        Ordinal = 3,
                        Color = BlueHexColorString,
                    },
                ],
            }
        );

        userProjects = await userClient.GetUserProjects();
        userProjects.ShouldBeEquivalentTo(
            new GetUserProjectsResponse(
                Projects:
                [
                    new(
                        Id: createdProjectOne.Id,
                        Name: "UserProjectCrudWorkflow1",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: invoiceItem1Id,
                                Name: "Item1",
                                Ordinal: 1,
                                Color: RedHexColorString
                            ),
                            new(
                                Id: invoiceItem2Id,
                                Name: "Item2",
                                Ordinal: 2,
                                Color: GreenHexColorString
                            ),
                            new(
                                Id: invoiceItem3Id,
                                Name: "Item3",
                                Ordinal: 3,
                                Color: BlueHexColorString
                            ),
                        ]
                    ),
                    new(
                        Id: createdProjectTwo.Id,
                        Name: "UserProjectCrudWorkflow2",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: invoiceItem6Id,
                                Name: "Item6",
                                Ordinal: 1,
                                Color: RedHexColorString
                            ),
                            new(
                                Id: invoiceItem5Id,
                                Name: "Item5",
                                Ordinal: 2,
                                Color: RedHexColorString
                            ),
                            new(
                                Id: invoiceItem4Id,
                                Name: "Item4",
                                Ordinal: 3,
                                Color: RedHexColorString
                            ),
                        ]
                    ),
                ]
            )
        );

        // Update 2: reset project 2 to NULL colors

        await userClient.UpdateUserInvoiceItemCustomizations(
            request: new()
            {
                InvoiceItems =
                [
                    new()
                    {
                        Id = invoiceItem1Id,
                        Ordinal = 1,
                        Color = RedHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem2Id,
                        Ordinal = 2,
                        Color = GreenHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem3Id,
                        Ordinal = 3,
                        Color = BlueHexColorString,
                    },
                    new()
                    {
                        Id = invoiceItem4Id,
                        Ordinal = 4,
                        Color = null,
                    },
                    new()
                    {
                        Id = invoiceItem5Id,
                        Ordinal = 5,
                        Color = null,
                    },
                    new()
                    {
                        Id = invoiceItem6Id,
                        Ordinal = 6,
                        Color = null,
                    },
                ],
            }
        );

        userProjects = await userClient.GetUserProjects();
        userProjects.ShouldBeEquivalentTo(
            new GetUserProjectsResponse(
                Projects:
                [
                    new(
                        Id: createdProjectOne.Id,
                        Name: "UserProjectCrudWorkflow1",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: invoiceItem1Id,
                                Name: "Item1",
                                Ordinal: 1,
                                Color: RedHexColorString
                            ),
                            new(
                                Id: invoiceItem2Id,
                                Name: "Item2",
                                Ordinal: 2,
                                Color: GreenHexColorString
                            ),
                            new(
                                Id: invoiceItem3Id,
                                Name: "Item3",
                                Ordinal: 3,
                                Color: BlueHexColorString
                            ),
                        ]
                    ),
                    new(
                        Id: createdProjectTwo.Id,
                        Name: "UserProjectCrudWorkflow2",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(Id: invoiceItem4Id, Name: "Item4", Ordinal: 4, Color: null),
                            new(Id: invoiceItem5Id, Name: "Item5", Ordinal: 5, Color: null),
                            new(Id: invoiceItem6Id, Name: "Item6", Ordinal: 6, Color: null),
                        ]
                    ),
                ]
            )
        );

        // No op

        await userClient.UpdateUserInvoiceItemCustomizations(request: new() { InvoiceItems = [] });

        userProjects = await userClient.GetUserProjects();
        userProjects.ShouldBeEquivalentTo(
            new GetUserProjectsResponse(
                Projects:
                [
                    new(
                        Id: createdProjectOne.Id,
                        Name: "UserProjectCrudWorkflow1",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(
                                Id: invoiceItem1Id,
                                Name: "Item1",
                                Ordinal: 1,
                                Color: RedHexColorString
                            ),
                            new(
                                Id: invoiceItem2Id,
                                Name: "Item2",
                                Ordinal: 2,
                                Color: GreenHexColorString
                            ),
                            new(
                                Id: invoiceItem3Id,
                                Name: "Item3",
                                Ordinal: 3,
                                Color: BlueHexColorString
                            ),
                        ]
                    ),
                    new(
                        Id: createdProjectTwo.Id,
                        Name: "UserProjectCrudWorkflow2",
                        Enabled: true,
                        InvoiceItems:
                        [
                            new(Id: invoiceItem4Id, Name: "Item4", Ordinal: 4, Color: null),
                            new(Id: invoiceItem5Id, Name: "Item5", Ordinal: 5, Color: null),
                            new(Id: invoiceItem6Id, Name: "Item6", Ordinal: 6, Color: null),
                        ]
                    ),
                ]
            )
        );
    }
}
