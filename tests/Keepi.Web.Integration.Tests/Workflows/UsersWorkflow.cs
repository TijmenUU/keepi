using Keepi.Api.Users.Get;
using Keepi.Api.Users.GetAll;
using Keepi.Api.Users.UpdatePermissions;

namespace Keepi.Web.Integration.Tests.Workflows;

[Collection(DefaultCollection.Name)]
public class UsersWorkflow(KeepiWebApplicationFactory applicationFactory)
{
    [Fact]
    public async Task Admin_user_returns_expected_values()
    {
        var client = await applicationFactory.CreateClientForAdminUser();

        var currentUser = await client.GetUser();
        currentUser.Name.ShouldBe("Bob");
        currentUser.EmailAddress.ShouldEndWith("@example.com"); // Sanity check, the e-mail address is hardcoded and thus of little interest
        currentUser.EntriesPermission.ShouldBe(GetUserResponsePermission.ReadAndModify);
        currentUser.ExportsPermission.ShouldBe(GetUserResponsePermission.ReadAndModify);
        currentUser.ProjectsPermission.ShouldBe(GetUserResponsePermission.ReadAndModify);
        currentUser.UsersPermission.ShouldBe(GetUserResponsePermission.ReadAndModify);
    }

    [Fact]
    public async Task New_user_can_register_with_the_application()
    {
        var subjectClaim = Guid.NewGuid().ToString();

        var client = await applicationFactory.CreateClientForNormalUser(
            fullName: "Henk de Vries",
            subjectClaim: subjectClaim
        );

        var currentUser = await client.GetUser();
        currentUser.Name.ShouldBe("Henk de Vries");
        currentUser.EmailAddress.ShouldBe($"{subjectClaim}@example.com");
        currentUser.EntriesPermission.ShouldBe(GetUserResponsePermission.ReadAndModify);
        currentUser.ExportsPermission.ShouldBe(GetUserResponsePermission.None);
        currentUser.ProjectsPermission.ShouldBe(GetUserResponsePermission.None);
        currentUser.UsersPermission.ShouldBe(GetUserResponsePermission.None);
    }

    [Fact]
    public async Task Create_user_and_update_user_permissions()
    {
        var adminClient = await applicationFactory.CreateClientForAdminUser();

        // Updating self should not be allowed
        var adminUser = await adminClient.GetUser();
        await Should.ThrowAsync<KeepiClientBadRequestException>(() =>
            adminClient.UpdateUserPermissions(
                userId: adminUser.Id,
                request: new()
                {
                    EntriesPermission = UpdateUserPermissionsRequestPermission.None,
                    ExportsPermission = UpdateUserPermissionsRequestPermission.None,
                    ProjectsPermission = UpdateUserPermissionsRequestPermission.None,
                    UsersPermission = UpdateUserPermissionsRequestPermission.None,
                }
            )
        );

        // Try all possible permission combinations and verify
        var userClient = await applicationFactory.CreateClientForRandomNormalUser();
        var normalUser = await userClient.GetUser();

        var combinations = new (
            UpdateUserPermissionsRequestPermission,
            UpdateUserPermissionsRequestPermission,
            UpdateUserPermissionsRequestPermission,
            UpdateUserPermissionsRequestPermission
        )[]
        {
            // Zero permissions case
            (
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None
            ),
            // Entry permission
            (
                UpdateUserPermissionsRequestPermission.Read,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None
            ),
            (
                UpdateUserPermissionsRequestPermission.ReadAndModify,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None
            ),
            // Export permission
            (
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.Read,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None
            ),
            (
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.ReadAndModify,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None
            ),
            // Project permission
            (
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.Read,
                UpdateUserPermissionsRequestPermission.None
            ),
            (
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.ReadAndModify,
                UpdateUserPermissionsRequestPermission.Read
            ),
            // Users permission
            (
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.Read
            ),
            (
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.None,
                UpdateUserPermissionsRequestPermission.ReadAndModify
            ),
        };

        foreach (var combination in combinations)
        {
            await UpdateUserPermissionsAndValidate(
                updateClient: adminClient,
                userClient: userClient,
                userId: normalUser.Id,
                entriesPermission: combination.Item1,
                exportsPermission: combination.Item2,
                projectsPermission: combination.Item3,
                usersPermission: combination.Item4
            );
        }
    }

    private static async Task UpdateUserPermissionsAndValidate(
        KeepiClient updateClient,
        KeepiClient userClient,
        int userId,
        UpdateUserPermissionsRequestPermission entriesPermission,
        UpdateUserPermissionsRequestPermission exportsPermission,
        UpdateUserPermissionsRequestPermission projectsPermission,
        UpdateUserPermissionsRequestPermission usersPermission
    )
    {
        await updateClient.UpdateUserPermissions(
            userId: userId,
            request: new()
            {
                EntriesPermission = entriesPermission,
                ExportsPermission = exportsPermission,
                ProjectsPermission = projectsPermission,
                UsersPermission = usersPermission,
            }
        );

        var getUserResponse = await userClient.GetUser();
        getUserResponse.EntriesPermission.ShouldBe(
            ConvertToEnumByName<UpdateUserPermissionsRequestPermission, GetUserResponsePermission>(
                entriesPermission
            )
        );
        getUserResponse.ExportsPermission.ShouldBe(
            ConvertToEnumByName<UpdateUserPermissionsRequestPermission, GetUserResponsePermission>(
                exportsPermission
            )
        );
        getUserResponse.ProjectsPermission.ShouldBe(
            ConvertToEnumByName<UpdateUserPermissionsRequestPermission, GetUserResponsePermission>(
                projectsPermission
            )
        );
        getUserResponse.UsersPermission.ShouldBe(
            ConvertToEnumByName<UpdateUserPermissionsRequestPermission, GetUserResponsePermission>(
                usersPermission
            )
        );

        var getAllUsersResponse = await updateClient.GetAllUsers();
        var getAllUser = getAllUsersResponse.Users.Single(u => u.Id == userId);
        getAllUser.EntriesPermission.ShouldBe(
            ConvertToEnumByName<
                UpdateUserPermissionsRequestPermission,
                GetAllUsersResponseUserPermission
            >(entriesPermission)
        );
        getAllUser.ExportsPermission.ShouldBe(
            ConvertToEnumByName<
                UpdateUserPermissionsRequestPermission,
                GetAllUsersResponseUserPermission
            >(exportsPermission)
        );
        getAllUser.ProjectsPermission.ShouldBe(
            ConvertToEnumByName<
                UpdateUserPermissionsRequestPermission,
                GetAllUsersResponseUserPermission
            >(projectsPermission)
        );
        getAllUser.UsersPermission.ShouldBe(
            ConvertToEnumByName<
                UpdateUserPermissionsRequestPermission,
                GetAllUsersResponseUserPermission
            >(usersPermission)
        );
    }

    private static TOutput ConvertToEnumByName<TInput, TOutput>(TInput input)
        where TInput : notnull
        where TOutput : notnull
    {
        var value = Enum.GetName(typeof(TInput), input);
        value.ShouldNotBeNull();
        return (TOutput)Enum.Parse(typeof(TOutput), value);
    }
}
