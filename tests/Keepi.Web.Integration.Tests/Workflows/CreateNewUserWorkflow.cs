using Keepi.Api.Users.Get;
using Keepi.Api.Users.Register;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class CreateNewUserWorkflow(KeepiWebApplicationFactory applicationFactory)
{
  [Fact]
  public async Task New_user_can_register_with_the_application()
  {
    var client = applicationFactory.CreateAuthorizedClient(
      userName: "Henk de Vries",
      userSubjectClaim: Guid.NewGuid().ToString());

    await ValidateGetUserEndpoint(client: client, expectedRegisteredValue: false);

    await RegisterUser(client);

    await ValidateGetUserEndpoint(client: client, expectedRegisteredValue: true);
  }

  private static async Task ValidateGetUserEndpoint(HttpClient client, bool expectedRegisteredValue)
  {
    var httpResponse = await client.GetAsync("/api/user");
    httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

    var getUserResponse = await httpResponse.Content.ReadFromJsonAsync<GetUserResponse>();
    getUserResponse.ShouldBeEquivalentTo(new GetUserResponse(
      name: "Henk de Vries",
      registered: expectedRegisteredValue));
  }

  private static async Task RegisterUser(HttpClient client)
  {
    var httpResponse = await client.PostAsync("/api/registeruser", new StringContent(string.Empty));
    httpResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);

    var postRegisterUserResponse = await httpResponse.Content.ReadFromJsonAsync<PostRegisterUserResponse>();
    postRegisterUserResponse.ShouldBeEquivalentTo(new PostRegisterUserResponse(
      result: PostRegisterUserResponseResult.Created));
  }
}