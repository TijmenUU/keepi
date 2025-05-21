using Keepi.Api.Endpoints.GetUserEntryCategories;
using Keepi.Api.Endpoints.PostCreateUserEntryCategory;
using Keepi.Api.Endpoints.PutUpdateUserEntryCategory;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UserEntryCategoryCrudWorkflow(KeepiWebApplicationFactory applicationFactory)
{
  [Fact]
  public async Task Create_read_update_delete_entry_category_test()
  {
    var client = applicationFactory.CreateAuthorizedClient(
      userName: "Paul de Groot",
      userSubjectClaim: Guid.NewGuid().ToString());

    await RegisterUser(client);

    var firstCreatedEntryCategoryId = await CreateEntryCategory(client: client, name: "Test 1");
    var secondCreatedEntryCategoryId = await CreateEntryCategory(client: client, name: "Test 2");

    firstCreatedEntryCategoryId.ShouldNotBe(secondCreatedEntryCategoryId);

    await VerifyGetEntryCategoriesResponse(
      client,
      new GetUserEntryCategoriesResponseCategory(
        Id: firstCreatedEntryCategoryId,
        Name: "Test 1",
        Enabled: true,
        ActiveFrom: null,
        ActiveTo: null),
      new GetUserEntryCategoriesResponseCategory(
        Id: secondCreatedEntryCategoryId,
        Name: "Test 2",
        Enabled: true,
        ActiveFrom: null,
        ActiveTo: null));

    var httpResponse = await client.PutAsJsonAsync(
      requestUri: $"/api/user/entrycategories/{firstCreatedEntryCategoryId}",
      value: new PutUpdateUserEntryCategoryRequest
      {
        Name = "Test 1a",
        ActiveFrom = "2025-01-01",
        ActiveTo = "2025-12-31",
        Enabled = false,
      });
    httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

    await VerifyGetEntryCategoriesResponse(
      client,
      new GetUserEntryCategoriesResponseCategory(
        Id: firstCreatedEntryCategoryId,
        Name: "Test 1a",
        Enabled: false,
        ActiveFrom: new DateOnly(2025, 1, 1),
        ActiveTo: new DateOnly(2025, 12, 31)),
      new GetUserEntryCategoriesResponseCategory(
        Id: secondCreatedEntryCategoryId,
        Name: "Test 2",
        Enabled: true,
        ActiveFrom: null,
        ActiveTo: null));

    var httpResponseMessage = await client.DeleteAsync(requestUri: $"/api/user/entrycategories/{firstCreatedEntryCategoryId}");
    httpResponseMessage.StatusCode.ShouldBe(System.Net.HttpStatusCode.NoContent);

    await VerifyGetEntryCategoriesResponse(
      client,
      new GetUserEntryCategoriesResponseCategory(
        Id: secondCreatedEntryCategoryId,
        Name: "Test 2",
        Enabled: true,
        ActiveFrom: null,
        ActiveTo: null));
  }

  private static async Task RegisterUser(HttpClient client)
  {
    var httpResponse = await client.PostAsync("/api/registeruser", new StringContent(string.Empty));
    httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
  }

  private static async Task<int> CreateEntryCategory(HttpClient client, string name)
  {
    var httpResponse = await client.PostAsJsonAsync(
      requestUri: "/api/user/entrycategories",
      value: new PostCreateUserEntryCategoryRequest
      {
        Name = name,
        ActiveFrom = null,
        ActiveTo = null,
        Enabled = true
      });
    httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
    httpResponse.Headers.Location?.OriginalString.ShouldBe("/api/user/entrycategories");

    var postCreateEntryCategoryResponse = await httpResponse.Content.ReadFromJsonAsync<PostCreateUserEntryCategoryResponse>();
    postCreateEntryCategoryResponse.ShouldNotBeNull();
    postCreateEntryCategoryResponse.Id.ShouldBeGreaterThan(0);

    return postCreateEntryCategoryResponse.Id;
  }

  private static async Task VerifyGetEntryCategoriesResponse(HttpClient client, params GetUserEntryCategoriesResponseCategory[] expectedEntryCategories)
  {
    var getUserEntryCategoriesResponse = await client.GetFromJsonAsync<GetUserEntryCategoriesResponse>("/api/user/entrycategories");
    getUserEntryCategoriesResponse.ShouldNotBeNull();
    getUserEntryCategoriesResponse.Categories.ShouldBeEquivalentTo(expectedEntryCategories);
  }
}