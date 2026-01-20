using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests;

internal class ResolvedUserBuilder
{
    public static ResolvedUser CreateAdministratorBob() =>
        new(
            Id: 42,
            Name: "Bob",
            EmailAddress: "bob@example.com",
            EntriesPermission: UserPermission.ReadAndModify,
            ExportsPermission: UserPermission.ReadAndModify,
            ProjectsPermission: UserPermission.ReadAndModify,
            UsersPermission: UserPermission.ReadAndModify
        );
}
