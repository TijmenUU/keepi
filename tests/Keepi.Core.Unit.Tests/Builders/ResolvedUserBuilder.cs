using Keepi.Core.Users;

namespace Keepi.Core.Unit.Tests.Builders;

internal class ResolvedUserBuilder
{
    private int id = 0;
    private string name = string.Empty;
    private string emailAddress = string.Empty;
    private UserPermission entriesPermission = UserPermission.None;
    private UserPermission exportsPermission = UserPermission.None;
    private UserPermission projectsPermission = UserPermission.None;
    private UserPermission usersPermission = UserPermission.None;

    public ResolvedUserBuilder(int id, string name, string emailAddress)
    {
        this.id = id;
        this.name = name;
        this.emailAddress = emailAddress;
    }

    public static ResolvedUserBuilder AsAdministratorBob()
    {
        return new ResolvedUserBuilder(id: 42, name: "Bob", emailAddress: "bob@example.com")
            .WithEntriesPermission(UserPermission.ReadAndModify)
            .WithExportsPermission(UserPermission.ReadAndModify)
            .WithProjectsPermission(UserPermission.ReadAndModify)
            .WithUsersPermission(UserPermission.ReadAndModify);
    }

    public ResolvedUserBuilder WithEntriesPermission(UserPermission value)
    {
        entriesPermission = value;
        return this;
    }

    public ResolvedUserBuilder WithExportsPermission(UserPermission value)
    {
        exportsPermission = value;
        return this;
    }

    public ResolvedUserBuilder WithProjectsPermission(UserPermission value)
    {
        projectsPermission = value;
        return this;
    }

    public ResolvedUserBuilder WithUsersPermission(UserPermission value)
    {
        usersPermission = value;
        return this;
    }

    public ResolvedUser Build() =>
        new(
            Id: id,
            Name: name,
            EmailAddress: emailAddress,
            EntriesPermission: entriesPermission,
            ExportsPermission: exportsPermission,
            ProjectsPermission: projectsPermission,
            UsersPermission: usersPermission
        );

    public static ResolvedUser CreateAdministratorBob() => AsAdministratorBob().Build();
}
