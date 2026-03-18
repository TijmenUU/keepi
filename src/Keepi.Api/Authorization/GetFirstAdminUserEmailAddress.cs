using Keepi.Core;
using Keepi.Core.Users;
using Microsoft.Extensions.Configuration;

namespace Keepi.Api.Authorization;

internal sealed class GetFirstAdminUserEmailAddress(IConfiguration configuration)
    : IGetFirstAdminUserEmailAddress
{
    public IValueOrErrorResult<EmailAddress, GetFirstAdminUserEmailAddressError> Execute()
    {
        const string configurationKey = "Authentication:FirstAdminUserEmailAddress";

        var configurationValue = configuration[configurationKey];
        if (!EmailAddress.TryFrom(value: configurationValue, out var emailAddress))
        {
            return Result.Failure<EmailAddress, GetFirstAdminUserEmailAddressError>(
                GetFirstAdminUserEmailAddressError.NotConfigured
            );
        }

        return Result.Success<EmailAddress, GetFirstAdminUserEmailAddressError>(emailAddress);
    }
}
