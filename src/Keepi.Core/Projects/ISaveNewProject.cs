namespace Keepi.Core.Projects;

public interface ISaveNewProject
{
    Task<IValueOrErrorResult<int, SaveNewProjectError>> Execute(
        string name,
        bool enabled,
        int[] userIds,
        string[] invoiceItemNames,
        CancellationToken cancellationToken
    );
}

public enum SaveNewProjectError
{
    Unknown = 0,
    DuplicateProjectName,
    UnknownUserId,
}
