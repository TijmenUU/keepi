using Keepi.Api.UserEntryCategories.GetAll;
using Keepi.Api.UserEntryCategories.UpdateAll;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UserUserEntryCategoryCrudWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Create_read_update_delete_entry_category_test()
    {
        var client = await applicationFactory.CreateRegisteredUserClient(
            userName: "Paul de Groot",
            userSubjectClaim: Guid.NewGuid().ToString()
        );

        await CreateUserEntryCategories(client, [("Test 1", 1), ("Test 2", 2)]);

        var getUserUserEntryCategoriesResponse =
            await client.GetFromJsonAsync<GetUserUserEntryCategoriesResponse>(
                requestUri: "/api/user/entrycategories",
                options: KeepiWebApplicationFactory.GetDefaultJsonSerializerOptions()
            );
        getUserUserEntryCategoriesResponse.ShouldNotBeNull();
        getUserUserEntryCategoriesResponse.Categories.ShouldContain(c => c.Name == "Test 1");
        getUserUserEntryCategoriesResponse.Categories.ShouldContain(c => c.Name == "Test 2");

        var firstCreatedUserEntryCategoryId = getUserUserEntryCategoriesResponse
            .Categories.Single(c => c.Name == "Test 1")
            .Id;
        var secondCreatedUserEntryCategoryId = getUserUserEntryCategoriesResponse
            .Categories.Single(c => c.Name == "Test 2")
            .Id;

        await VerifyGetUserEntryCategoriesResponse(
            client,
            new GetUserUserEntryCategoriesResponseCategory(
                Id: firstCreatedUserEntryCategoryId,
                Name: "Test 1",
                Ordinal: 1,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            ),
            new GetUserUserEntryCategoriesResponseCategory(
                Id: secondCreatedUserEntryCategoryId,
                Name: "Test 2",
                Ordinal: 2,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            )
        );

        // Should update Test 1 entry category only
        await UpdateUserEntryCategories(
            client: client,
            values:
            [
                new()
                {
                    Id = firstCreatedUserEntryCategoryId,
                    Name = "Test 1a",
                    Ordinal = 3,
                    ActiveFrom = "2025-01-01",
                    ActiveTo = "2025-12-31",
                    Enabled = false,
                },
                new()
                {
                    Id = secondCreatedUserEntryCategoryId,
                    Name = "Test 2",
                    Ordinal = 2,
                    ActiveFrom = null,
                    ActiveTo = null,
                    Enabled = true,
                },
            ]
        );

        await VerifyGetUserEntryCategoriesResponse(
            client,
            new GetUserUserEntryCategoriesResponseCategory(
                Id: firstCreatedUserEntryCategoryId,
                Name: "Test 1a",
                Ordinal: 3,
                Enabled: false,
                ActiveFrom: new DateOnly(2025, 1, 1),
                ActiveTo: new DateOnly(2025, 12, 31)
            ),
            new GetUserUserEntryCategoriesResponseCategory(
                Id: secondCreatedUserEntryCategoryId,
                Name: "Test 2",
                Ordinal: 2,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            )
        );

        // Should delete Test 1a entry category only
        await UpdateUserEntryCategories(
            client: client,
            values:
            [
                new()
                {
                    Id = secondCreatedUserEntryCategoryId,
                    Name = "Test 2",
                    Ordinal = 2,
                    ActiveFrom = null,
                    ActiveTo = null,
                    Enabled = true,
                },
            ]
        );

        await VerifyGetUserEntryCategoriesResponse(
            client,
            new GetUserUserEntryCategoriesResponseCategory(
                Id: secondCreatedUserEntryCategoryId,
                Name: "Test 2",
                Ordinal: 2,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            )
        );
    }

    [Fact]
    public async Task Swap_ordinals_test()
    {
        var client = await applicationFactory.CreateRegisteredUserClient(
            userName: "Yannick Meijer",
            userSubjectClaim: Guid.NewGuid().ToString()
        );

        await CreateUserEntryCategories(client, [("Test 1", 1), ("Test 2", 2)]);

        var getUserUserEntryCategoriesResponse =
            await client.GetFromJsonAsync<GetUserUserEntryCategoriesResponse>(
                requestUri: "/api/user/entrycategories",
                options: KeepiWebApplicationFactory.GetDefaultJsonSerializerOptions()
            );
        getUserUserEntryCategoriesResponse.ShouldNotBeNull();
        getUserUserEntryCategoriesResponse.Categories.ShouldContain(c => c.Name == "Test 1");
        getUserUserEntryCategoriesResponse.Categories.ShouldContain(c => c.Name == "Test 2");

        var firstCreatedUserEntryCategoryId = getUserUserEntryCategoriesResponse
            .Categories.Single(c => c.Name == "Test 1")
            .Id;
        var secondCreatedUserEntryCategoryId = getUserUserEntryCategoriesResponse
            .Categories.Single(c => c.Name == "Test 2")
            .Id;

        await VerifyGetUserEntryCategoriesResponse(
            client,
            new GetUserUserEntryCategoriesResponseCategory(
                Id: firstCreatedUserEntryCategoryId,
                Name: "Test 1",
                Ordinal: 1,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            ),
            new GetUserUserEntryCategoriesResponseCategory(
                Id: secondCreatedUserEntryCategoryId,
                Name: "Test 2",
                Ordinal: 2,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            )
        );

        // Swap the ordinals of the two entry categories
        await UpdateUserEntryCategories(
            client: client,
            values:
            [
                new()
                {
                    Id = firstCreatedUserEntryCategoryId,
                    Name = "Test 1",
                    Ordinal = 2,
                    ActiveFrom = null,
                    ActiveTo = null,
                    Enabled = true,
                },
                new()
                {
                    Id = secondCreatedUserEntryCategoryId,
                    Name = "Test 2",
                    Ordinal = 1,
                    ActiveFrom = null,
                    ActiveTo = null,
                    Enabled = true,
                },
            ]
        );

        await VerifyGetUserEntryCategoriesResponse(
            client,
            new GetUserUserEntryCategoriesResponseCategory(
                Id: firstCreatedUserEntryCategoryId,
                Name: "Test 1",
                Ordinal: 2,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            ),
            new GetUserUserEntryCategoriesResponseCategory(
                Id: secondCreatedUserEntryCategoryId,
                Name: "Test 2",
                Ordinal: 1,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            )
        );
    }

    [Fact]
    public async Task Swap_names_test()
    {
        var client = await applicationFactory.CreateRegisteredUserClient(
            userName: "Yannick Meijer",
            userSubjectClaim: Guid.NewGuid().ToString()
        );

        await CreateUserEntryCategories(client, [("Test 1", 1), ("Test 2", 2)]);

        var getUserUserEntryCategoriesResponse =
            await client.GetFromJsonAsync<GetUserUserEntryCategoriesResponse>(
                requestUri: "/api/user/entrycategories",
                options: KeepiWebApplicationFactory.GetDefaultJsonSerializerOptions()
            );
        getUserUserEntryCategoriesResponse.ShouldNotBeNull();
        getUserUserEntryCategoriesResponse.Categories.ShouldContain(c => c.Name == "Test 1");
        getUserUserEntryCategoriesResponse.Categories.ShouldContain(c => c.Name == "Test 2");

        var firstCreatedUserEntryCategoryId = getUserUserEntryCategoriesResponse
            .Categories.Single(c => c.Name == "Test 1")
            .Id;
        var secondCreatedUserEntryCategoryId = getUserUserEntryCategoriesResponse
            .Categories.Single(c => c.Name == "Test 2")
            .Id;

        await VerifyGetUserEntryCategoriesResponse(
            client,
            new GetUserUserEntryCategoriesResponseCategory(
                Id: firstCreatedUserEntryCategoryId,
                Name: "Test 1",
                Ordinal: 1,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            ),
            new GetUserUserEntryCategoriesResponseCategory(
                Id: secondCreatedUserEntryCategoryId,
                Name: "Test 2",
                Ordinal: 2,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            )
        );

        // Swap the ordinals of the two entry categories
        await UpdateUserEntryCategories(
            client: client,
            values:
            [
                new()
                {
                    Id = firstCreatedUserEntryCategoryId,
                    Name = "Test 2",
                    Ordinal = 1,
                    ActiveFrom = null,
                    ActiveTo = null,
                    Enabled = true,
                },
                new()
                {
                    Id = secondCreatedUserEntryCategoryId,
                    Name = "Test 1",
                    Ordinal = 2,
                    ActiveFrom = null,
                    ActiveTo = null,
                    Enabled = true,
                },
            ]
        );

        await VerifyGetUserEntryCategoriesResponse(
            client,
            new GetUserUserEntryCategoriesResponseCategory(
                Id: firstCreatedUserEntryCategoryId,
                Name: "Test 2",
                Ordinal: 1,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            ),
            new GetUserUserEntryCategoriesResponseCategory(
                Id: secondCreatedUserEntryCategoryId,
                Name: "Test 1",
                Ordinal: 2,
                Enabled: true,
                ActiveFrom: null,
                ActiveTo: null
            )
        );
    }

    private static async Task CreateUserEntryCategories(
        HttpClient client,
        (string name, int ordinal)[] values
    )
    {
        await UpdateUserEntryCategories(
            client: client,
            values: values
                .Select(v => new PutUpdateUserEntryCategoriesRequestCategory
                {
                    Name = v.name,
                    Ordinal = v.ordinal,
                    ActiveFrom = null,
                    ActiveTo = null,
                    Enabled = true,
                })
                .ToArray()
        );
    }

    private static async Task UpdateUserEntryCategories(
        HttpClient client,
        PutUpdateUserEntryCategoriesRequestCategory[] values
    )
    {
        var httpResponse = await client.PutAsJsonAsync(
            requestUri: "/api/user/entrycategories",
            value: new PutUpdateUserEntryCategoriesRequest { UserEntryCategories = values },
            options: KeepiWebApplicationFactory.GetDefaultJsonSerializerOptions()
        );
        httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);
    }

    private static async Task VerifyGetUserEntryCategoriesResponse(
        HttpClient client,
        params GetUserUserEntryCategoriesResponseCategory[] expectedUserEntryCategories
    )
    {
        var getUserUserEntryCategoriesResponse =
            await client.GetFromJsonAsync<GetUserUserEntryCategoriesResponse>(
                requestUri: "/api/user/entrycategories",
                options: KeepiWebApplicationFactory.GetDefaultJsonSerializerOptions()
            );
        getUserUserEntryCategoriesResponse.ShouldNotBeNull();
        getUserUserEntryCategoriesResponse.Categories.ShouldBeEquivalentTo(
            expectedUserEntryCategories
        );
    }
}
