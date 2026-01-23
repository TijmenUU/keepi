namespace Keepi.Core.Users;

public interface IGetFirstAdminUserEmailAddress
{
    public IValueOrErrorResult<string, GetFirstAdminUserEmailAddressError> Execute();
}

public enum GetFirstAdminUserEmailAddressError
{
    Unknown = 0,
    NotConfigured,
}
