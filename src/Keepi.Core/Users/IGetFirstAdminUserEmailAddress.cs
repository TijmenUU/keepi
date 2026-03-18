namespace Keepi.Core.Users;

public interface IGetFirstAdminUserEmailAddress
{
    public IValueOrErrorResult<EmailAddress, GetFirstAdminUserEmailAddressError> Execute();
}

public enum GetFirstAdminUserEmailAddressError
{
    Unknown = 0,
    NotConfigured,
}
