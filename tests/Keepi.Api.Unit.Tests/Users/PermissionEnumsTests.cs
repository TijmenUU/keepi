using Keepi.Api.Users.Get;
using Keepi.Api.Users.GetAll;
using Keepi.Api.Users.UpdatePermissions;
using Keepi.Core.Users;

namespace Keepi.Api.Unit.Tests.Users;

public class PermissionEnumsTests
{
    [Fact]
    public void Permission_enums_have_equal_members()
    {
        Type[] enumTypes =
        [
            typeof(UpdateUserPermissionsRequestPermission),
            typeof(GetAllUsersResponseUserPermission),
            typeof(GetUserResponsePermission),
        ];

        var expectedValues = Enum.GetValues<UserPermission>().Select(v => v.ToString()).ToArray();

        foreach (var enumType in enumTypes)
        {
            var values = Enum.GetNames(enumType);
            values.Length.ShouldBe(3);
            values.SequenceEqual(expectedValues);
        }
    }
}
