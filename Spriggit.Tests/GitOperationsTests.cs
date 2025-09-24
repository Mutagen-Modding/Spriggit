using System.IO.Abstractions;
using LibGit2Sharp;
using Noggog;
using Noggog.GitRepository;
using Noggog.IO;
using NSubstitute;
using Serilog;
using Shouldly;
using Spriggit.Engine.Services.Singletons;
using Xunit;

namespace Spriggit.Tests;

public class GitOperationsTests : IDisposable
{
    private readonly ILogger _logger;
    private readonly GitRepositoryFactory _gitRepositoryFactory;
    private readonly GitFolderLocator _gitFolderLocator;
    private readonly IFileSystem _fileSystem;
    private readonly GitOperations _sut;
    private readonly TempFolder _repoFolder;

    public GitOperationsTests()
    {
        _logger = Substitute.For<ILogger>();
        _fileSystem = new FileSystem();
        _gitRepositoryFactory = new GitRepositoryFactory();
        _gitFolderLocator = new GitFolderLocator(_fileSystem);

        _repoFolder = TempFolder.Factory();

        _sut = new GitOperations(_logger, _gitRepositoryFactory, _gitFolderLocator);
    }

    public void Dispose()
    {
        _repoFolder?.Dispose();
    }

    #region HasUncommittedChanges Tests

    [Fact]
    public async Task HasUncommittedChanges_WithNoGitFolder_ReturnsFalseAndLogsWarning()
    {
        // Arrange - use a directory that doesn't have a git repo
        var nonGitDir = Path.Combine(_repoFolder.Dir, "non-git");
        Directory.CreateDirectory(nonGitDir);

        // Act
        var result = await _sut.HasUncommittedChanges(nonGitDir);

        // Assert
        result.ShouldBeFalse();
        _logger.Received(1).Warning("No git repository found for directory {WorkingDirectory}", nonGitDir);
    }

    [Fact]
    public async Task HasUncommittedChanges_WithCleanRepository_ReturnsFalse()
    {
        // Arrange - create a git repo with no changes
        Repository.Init(_repoFolder.Dir);

        using var repo = new Repository(_repoFolder.Dir);
        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);

        // Create initial commit
        var testFile = Path.Combine(_repoFolder.Dir, "test.txt");
        File.WriteAllText(testFile, "initial content");
        Commands.Stage(repo, "test.txt");
        repo.Commit("Initial commit", signature, signature);

        // Act
        var result = await _sut.HasUncommittedChanges(_repoFolder.Dir);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task HasUncommittedChanges_WithUncommittedChanges_ReturnsTrue()
    {
        // Arrange - create a git repo with uncommitted changes
        Repository.Init(_repoFolder.Dir);

        using var repo = new Repository(_repoFolder.Dir);
        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);

        // Create initial commit
        var testFile = Path.Combine(_repoFolder.Dir, "test.txt");
        File.WriteAllText(testFile, "initial content");
        Commands.Stage(repo, "test.txt");
        repo.Commit("Initial commit", signature, signature);

        // Make uncommitted changes
        File.WriteAllText(testFile, "modified content");

        // Act
        var result = await _sut.HasUncommittedChanges(_repoFolder.Dir);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task HasUncommittedChanges_WithUntrackedFiles_ReturnsTrue()
    {
        // Arrange - create a git repo with untracked files
        Repository.Init(_repoFolder.Dir);

        using var repo = new Repository(_repoFolder.Dir);
        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);

        // Create initial commit
        var testFile = Path.Combine(_repoFolder.Dir, "test.txt");
        File.WriteAllText(testFile, "initial content");
        Commands.Stage(repo, "test.txt");
        repo.Commit("Initial commit", signature, signature);

        // Add untracked file
        var untrackedFile = Path.Combine(_repoFolder.Dir, "untracked.txt");
        File.WriteAllText(untrackedFile, "untracked content");

        // Act
        var result = await _sut.HasUncommittedChanges(_repoFolder.Dir);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task HasUncommittedChanges_WithException_ReturnsFalseAndLogsError()
    {
        // Arrange - use a directory that will cause issues
        var invalidDir = Path.Combine(_repoFolder.Dir, "invalid-git");
        Directory.CreateDirectory(invalidDir);

        // Create a .git directory but not a proper repo (will cause LibGit2Sharp to fail)
        var gitDir = Path.Combine(invalidDir, ".git");
        Directory.CreateDirectory(gitDir);

        // Act
        var result = await _sut.HasUncommittedChanges(invalidDir);

        // Assert
        result.ShouldBeFalse();
        _logger.Received(1).Error(Arg.Any<Exception>(), "Error checking for uncommitted changes in {WorkingDirectory}", invalidDir);
    }

    #endregion

    #region CommitChanges Tests

    [Fact]
    public async Task CommitChanges_WithNoGitFolder_ReturnsFalse()
    {
        // Arrange - use a directory that doesn't have a git repo
        var nonGitDir = Path.Combine(_repoFolder.Dir, "non-git-commit");
        Directory.CreateDirectory(nonGitDir);
        var commitMessage = "Test commit";

        // Act
        var result = await _sut.CommitChanges(nonGitDir, commitMessage);

        // Assert
        result.ShouldBeFalse();
        _logger.Received(1).Error("No git repository found for directory {WorkingDirectory}", nonGitDir);
    }

    [Fact]
    public async Task CommitChanges_WithSuccessfulCommit_ReturnsTrue()
    {
        // Arrange - create a git repo with changes to commit
        Repository.Init(_repoFolder.Dir);

        using var repo = new Repository(_repoFolder.Dir);
        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);

        // Create initial commit
        var testFile = Path.Combine(_repoFolder.Dir, "test.txt");
        File.WriteAllText(testFile, "initial content");
        Commands.Stage(repo, "test.txt");
        repo.Commit("Initial commit", signature, signature);

        // Make changes
        File.WriteAllText(testFile, "modified content");
        var newFile = Path.Combine(_repoFolder.Dir, "new.txt");
        File.WriteAllText(newFile, "new file content");

        var commitMessage = "Test commit message";

        // Act
        var result = await _sut.CommitChanges(_repoFolder.Dir, commitMessage);

        // Assert
        result.ShouldBeTrue();
        _logger.Received(1).Information("Successfully committed changes with message: {CommitMessage}", commitMessage);

        // Verify the commit was actually made
        var lastCommit = repo.Head.Tip;
        lastCommit.Message.TrimEnd().ShouldBe(commitMessage);

        // Verify repo is clean after commit
        repo.RetrieveStatus().IsDirty.ShouldBeFalse();
    }

    [Fact]
    public async Task CommitChanges_WithException_ReturnsFalseAndLogsError()
    {
        // Arrange - use a directory that will cause issues
        var invalidDir = Path.Combine(_repoFolder.Dir, "invalid-git-commit");
        Directory.CreateDirectory(invalidDir);

        // Create a .git directory but not a proper repo (will cause LibGit2Sharp to fail)
        var gitDir = Path.Combine(invalidDir, ".git");
        Directory.CreateDirectory(gitDir);

        var commitMessage = "Test commit";

        // Act
        var result = await _sut.CommitChanges(invalidDir, commitMessage);

        // Assert
        result.ShouldBeFalse();
        _logger.Received(1).Error(Arg.Any<Exception>(), "Error committing changes in {WorkingDirectory}", invalidDir);
    }

    #endregion
}