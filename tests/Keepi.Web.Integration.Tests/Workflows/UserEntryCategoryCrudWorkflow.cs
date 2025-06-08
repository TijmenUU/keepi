using Keepi.Api.UserEntryCategories.Create;
using Keepi.Api.UserEntryCategories.GetAll;
using Keepi.Api.UserEntryCategories.Update;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UserUserEntryCategoryCrudWorkflow(KeepiWebApplicationFactory applicationFactory)
{
  [Fact]
  public async Task Create_read_update_delete_entry_category_test()
  {
    var client = await applicationFactory.CreateRegisteredUserClient(
      userName: "Paul de Groot",
      userSubjectClaim: Guid.NewGuid().ToString());

    var firstCreatedUserEntryCategoryId = await CreateUserEntryCategory(client: client, name: "Test 1");
    var secondCreatedUserEntryCategoryId = await CreateUserEntryCategory(client: client, name: "Test 2");

    firstCreatedUserEntryCategoryId.ShouldNotBe(secondCreatedUserEntryCategoryId);

    await VerifyGetUserEntryCategoriesResponse(
      client,
      new GetUserUserEntryCategoriesResponseCategory(
        Id: firstCreatedUserEntryCategoryId,
        Name: "Test 1",
        Enabled: true,
        ActiveFrom: null,
        ActiveTo: null),
      new GetUserUserEntryCategoriesResponseCategory(
        Id: secondCreatedUserEntryCategoryId,
        Name: "Test 2",
        Enabled: true,
        ActiveFrom: null,
        ActiveTo: null));

    var httpResponse = await client.PutAsJsonAsync(
      requestUri: $"/api/user/entrycategories/{firstCreatedUserEntryCategoryId}",
      value: new PutUpdateUserUserEntryCategoryRequest
      {
        Name = "Test 1a",
        ActiveFrom = "2025-01-01",
        ActiveTo = "2025-12-31",
        Enabled = false,
      });
    httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

    await VerifyGetUserEntryCategoriesResponse(
      client,
      new GetUserUserEntryCategoriesResponseCategory(
        Id: firstCreatedUserEntryCategoryId,
        Name: "Test 1a",
        Enabled: false,
        ActiveFrom: new DateOnly(2025, 1, 1),
        ActiveTo: new DateOnly(2025, 12, 31)),
      new GetUserUserEntryCategoriesResponseCategory(
        Id: secondCreatedUserEntryCategoryId,
        Name: "Test 2",
        Enabled: true,
        ActiveFrom: null,
        ActiveTo: null));

    var httpResponseMessage = await client.DeleteAsync(requestUri: $"/api/user/entrycategories/{firstCreatedUserEntryCategoryId}");
    httpResponseMessage.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

    await VerifyGetUserEntryCategoriesResponse(
      client,
      new GetUserUserEntryCategoriesResponseCategory(
        Id: secondCreatedUserEntryCategoryId,
        Name: "Test 2",
        Enabled: true,
        ActiveFrom: null,
        ActiveTo: null));
  }

  private static async Task<int> CreateUserEntryCategory(HttpClient client, string name)
  {
    var httpResponse = await client.PostAsJsonAsync(
      requestUri: "/api/user/entrycategories",
      value: new PostCreateUserUserEntryCategoryRequest
      {
        Name = name,
        ActiveFrom = null,
        ActiveTo = null,
        Enabled = true
      });
    httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
    httpResponse.Headers.Location?.OriginalString.ShouldBe("/api/user/entrycategories");

    var postCreateUserEntryCategoryResponse = await httpResponse.Content.ReadFromJsonAsync<PostCreateUserUserEntryCategoryResponse>();
    postCreateUserEntryCategoryResponse.ShouldNotBeNull();
    postCreateUserEntryCategoryResponse.Id.ShouldBeGreaterThan(0);

    return postCreateUserEntryCategoryResponse.Id;
  }

  private static async Task VerifyGetUserEntryCategoriesResponse(HttpClient client, params GetUserUserEntryCategoriesResponseCategory[] expectedUserEntryCategories)
  {
    var getUserUserEntryCategoriesResponse = await client.GetFromJsonAsync<GetUserUserEntryCategoriesResponse>("/api/user/entrycategories");
    getUserUserEntryCategoriesResponse.ShouldNotBeNull();
    getUserUserEntryCategoriesResponse.Categories.ShouldBeEquivalentTo(expectedUserEntryCategories);
  }
}