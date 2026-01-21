using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests.Users;

public class UserPermissionExtensionsTests
{
    [Theory]
    [InlineData(UserPermission.None, false)]
    [InlineData(UserPermission.Read, true)]
    [InlineData(UserPermission.ReadAndModify, true)]
    public void CanRead_returns_expected_value(UserPermission input, bool expectedResult)
    {
        input.CanRead().ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(UserPermission.None, false)]
    [InlineData(UserPermission.Read, false)]
    [InlineData(UserPermission.ReadAndModify, true)]
    public void CanModify_returns_expected_value(UserPermission input, bool expectedResult)
    {
        input.CanModify().ShouldBe(expectedResult);
    }
}
