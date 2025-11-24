namespace Keepi.Core.Projects;

public interface IUpdateProject
{
    Task<IMaybeErrorResult<UpdateProjectError>> Execute(
        int id,
        string name,
        bool enabled,
        int[] userIds,
        (int? Id, string Name)[] invoiceItems,
        CancellationToken cancellationToken
    );
}

public enum UpdateProjectError
{
    Unknown = 0,
    UnknownProjectId,
    DuplicateProjectName,
    UnknownUserId,
}
