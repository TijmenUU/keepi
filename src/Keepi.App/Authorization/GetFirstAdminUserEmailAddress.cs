using Keepi.Core;
using Keepi.Core.Users;

namespace Keepi.App.Authorization;

internal sealed class GetFirstAdminUserEmailAddress : IGetFirstAdminUserEmailAddress
{
    public const string LocalUserEmailAddress = "user@localhost";

    public IValueOrErrorResult<string, GetFirstAdminUserEmailAddressError> Execute()
    {
        return Result.Success<string, GetFirstAdminUserEmailAddressError>(LocalUserEmailAddress);
    }
}
