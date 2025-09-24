using Noggog.GitRepository;
using Serilog;

namespace Spriggit.Engine.Services.Singletons;

public class GitOperations
{
    private readonly ILogger _logger;
    private readonly IGitRepositoryFactory _gitRepositoryFactory;
    private readonly GitFolderLocator _gitFolderLocator;

    public GitOperations(
        ILogger logger,
        IGitRepositoryFactory gitRepositoryFactory,
        GitFolderLocator gitFolderLocator)
    {
        _logger = logger;
        _gitRepositoryFactory = gitRepositoryFactory;
        _gitFolderLocator = gitFolderLocator;
    }

    public async Task<bool> HasUncommittedChanges(string workingDirectory, CancellationToken cancellationToken = default)
    {
        var gitFolder = _gitFolderLocator.Get(workingDirectory);
        if (gitFolder == null)
        {
            _logger.Warning("No git repository found for directory {WorkingDirectory}", workingDirectory);
            return false;
        }

        var repoRoot = Path.GetDirectoryName(gitFolder.Value);
        if (repoRoot == null)
        {
            _logger.Warning("Could not determine git repository root for {GitFolder}", gitFolder);
            return false;
        }

        try
        {
            using var repository = _gitRepositoryFactory.Get(repoRoot);
            return repository.HasUncommittedChanges;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking for uncommitted changes in {WorkingDirectory}", workingDirectory);
            return false;
        }
    }

    public async Task<bool> CommitChanges(string workingDirectory, string commitMessage, CancellationToken cancellationToken = default)
    {
        var gitFolder = _gitFolderLocator.Get(workingDirectory);
        if (gitFolder == null)
        {
            _logger.Error("No git repository found for directory {WorkingDirectory}", workingDirectory);
            return false;
        }

        var repoRoot = Path.GetDirectoryName(gitFolder.Value);
        if (repoRoot == null)
        {
            _logger.Error("Could not determine git repository root for {GitFolder}", gitFolder);
            return false;
        }

        try
        {
            using var repository = _gitRepositoryFactory.Get(repoRoot);

            // Stage all changes
            repository.Stage("*");

            // Commit the changes
            repository.Commit(commitMessage);

            _logger.Information("Successfully committed changes with message: {CommitMessage}", commitMessage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error committing changes in {WorkingDirectory}", workingDirectory);
            return false;
        }
    }
}