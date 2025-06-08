namespace Keepi.Web.Integration.Tests;

[CollectionDefinition(Name)]
public class DefaultCollection : ICollectionFixture<KeepiWebApplicationFactory>
{
    internal const string Name = "Default";
}
