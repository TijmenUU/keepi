namespace Keepi.Infrastructure.Data;

public interface IEnsureDatabaseCreated
{
    void Execute();
}

internal sealed class EnsureDatabaseCreated(DatabaseContext databaseContext)
    : IEnsureDatabaseCreated
{
    public void Execute()
    {
        // It would be nice to check if the database schema is as expected when
        // this returns false but there does not seem to be such a thing in EF
        // Core.
        databaseContext.Database.EnsureCreated();
    }
}
