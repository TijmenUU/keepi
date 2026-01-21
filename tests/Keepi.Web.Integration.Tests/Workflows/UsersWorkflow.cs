using Keepi.Api.Users.Get;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UsersWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task New_user_can_register_with_the_application()
    {
        var subjectClaim = Guid.NewGuid().ToString();

        var client = new KeepiClient(httpClient: applicationFactory.CreateClient());
        client.ConfigureUser(name: "Henk de Vries", subjectClaim: subjectClaim);

        var currentUser = await client.GetUser();
        currentUser.Name.ShouldBe("Henk de Vries");
        currentUser.EmailAddress.ShouldBe($"{subjectClaim}@example.com");
        currentUser.EntriesPermission.ShouldBe(GetUserResponsePermission.ReadAndModify);
        currentUser.ExportsPermission.ShouldBe(GetUserResponsePermission.ReadAndModify);
        currentUser.ProjectsPermission.ShouldBe(GetUserResponsePermission.ReadAndModify);
        currentUser.UsersPermission.ShouldBe(GetUserResponsePermission.ReadAndModify);
    }
}
