namespace Keepi.App.Authorization;

internal interface IGetUserName
{
    string Execute();
}

// TODO consider using platform specific implementations
internal sealed class GetUserName : IGetUserName
{
    public string Execute() => System.Environment.UserName;
}
