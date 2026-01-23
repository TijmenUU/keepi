using Keepi.Core;
using Keepi.Core.Users;
using Microsoft.Extensions.Configuration;

namespace Keepi.Api.Authorization;

internal sealed class GetFirstAdminUserEmailAddress(IConfiguration configuration)
    : IGetFirstAdminUserEmailAddress
{
    public IValueOrErrorResult<string, GetFirstAdminUserEmailAddressError> Execute()
    {
        const string configurationKey = "Authentication:FirstAdminUserEmailAddress";

        var emailAddress = configuration[configurationKey];
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            return Result.Failure<string, GetFirstAdminUserEmailAddressError>(
                GetFirstAdminUserEmailAddressError.NotConfigured
            );
        }

        return Result.Success<string, GetFirstAdminUserEmailAddressError>(emailAddress);
    }
}
