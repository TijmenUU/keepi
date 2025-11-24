namespace Keepi.Core.Projects;

public interface IDeleteProject
{
    Task<IMaybeErrorResult<DeleteProjectError>> Execute(
        int projectId,
        CancellationToken cancellationToken
    );
}

public enum DeleteProjectError
{
    Unknown = 0,
    UnknownProjectId,
}
