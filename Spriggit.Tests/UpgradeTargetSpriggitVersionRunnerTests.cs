using System.IO.Abstractions;
using AutoFixture.Xunit2;
using LibGit2Sharp;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Testing.AutoData;
using Newtonsoft.Json;
using Noggog;
using Noggog.GitRepository;
using Noggog.IO;
using NSubstitute;
using Serilog;
using Shouldly;
using Spriggit.CLI.Lib;
using Spriggit.CLI.Lib.Commands;
using Spriggit.CLI.Lib.Commands.UpgradeTargetSpriggitVersionCommand;
using Spriggit.Core;
using Spriggit.Engine;
using Spriggit.Engine.Services.Singletons;
using Xunit;

namespace Spriggit.Tests;

public class UpgradeTargetSpriggitVersionRunnerTests : IDisposable
{
    private readonly TempFolder _tempFolder;
    private readonly IFileSystem _fileSystem;
    private readonly GitRepositoryFactory _gitRepositoryFactory;
    private readonly GitFolderLocator _gitFolderLocator;
    private readonly GitOperations _gitOperations;
    private readonly ILogger _logger;

    public UpgradeTargetSpriggitVersionRunnerTests()
    {
        _tempFolder = TempFolder.Factory();
        _fileSystem = new FileSystem();
        _gitRepositoryFactory = new GitRepositoryFactory();
        _gitFolderLocator = new GitFolderLocator(_fileSystem);
        _logger = Substitute.For<ILogger>();
        _gitOperations = new GitOperations(_logger, _gitRepositoryFactory, _gitFolderLocator);
    }

    public void Dispose()
    {
        _tempFolder?.Dispose();
    }

    private DirectoryPath CreateGitRepository(bool withInitialCommit = true, bool withUncommittedChanges = false)
    {
        var repoPath = Path.Combine(_tempFolder.Dir, Path.GetRandomFileName());
        Directory.CreateDirectory(repoPath);

        Repository.Init(repoPath);

        if (withInitialCommit)
        {
            using var repo = new Repository(repoPath);
            var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);

            var testFile = Path.Combine(repoPath, "test.txt");
            File.WriteAllText(testFile, "initial content");
            Commands.Stage(repo, "test.txt");
            repo.Commit("Initial commit", signature, signature);

            if (withUncommittedChanges)
            {
                File.WriteAllText(testFile, "modified content");
            }
        }

        return new DirectoryPath(repoPath);
    }

    [Theory, MutagenAutoData]
    public async Task Execute_WithNullMeta_ReturnsError(
        UpgradeTargetSpriggitVersionCommand command,
        [Frozen] ILogger logger,
        UpgradeTargetSpriggitVersionRunner sut)
    {
        // Act
        var result = await sut.Execute(command);

        // Assert
        // No meta file exists in the file system, so TryParseEmbeddedMeta will return null
        result.ShouldBe(1);
        logger.Received(1).Error(
            "Could not parse existing spriggit-meta.json in {SpriggitPath}",
            command.SpriggitPath);
    }
    
    [Theory, MutagenAutoData]
    public async Task Execute_WithSpecificPackageVersion_UpdatesMetaVersion(
        DirectoryPath existingsSpriggitPath,
        ModKey modKey,
        [Frozen] ILogger logger,
        [Frozen] ISpriggitEngine engine,
        UpgradeTargetSpriggitVersionRunner sut)
    {
        // Arrange
        // Create the directory first
        Directory.CreateDirectory(existingsSpriggitPath);

        var command = new UpgradeTargetSpriggitVersionCommand
        {
            SkipGitOperations = true, // Skip git operations for this test
            PackageVersion = "0.40.0",
            SpriggitPath = existingsSpriggitPath,
            DataFolder = Path.Combine(existingsSpriggitPath, "Data")
        };

        var originalMeta = new SpriggitModKeyMeta(
            new SpriggitSource { PackageName = "Spriggit.Yaml.Starfield", Version = "0.39.11" },
            GameRelease.Starfield,
            modKey);

        var persister = new SpriggitExternalMetaPersister(_fileSystem);
        persister.Persist(command.SpriggitPath, originalMeta);

        // Create runner with real dependencies
        var metaUpdater = new SpriggitMetaUpdater(logger, _fileSystem, persister);
        var runner = new UpgradeTargetSpriggitVersionRunner(
            engine,
            metaUpdater,
            _fileSystem,
            persister,
            _gitOperations,
            logger);

        // Act
        var result = await runner.Execute(command);
    
        // Assert
        result.ShouldBe(0);
        logger.Received(1).Information("Updating spriggit-meta.json to version {Version}", command.PackageVersion);
    
        // Verify engine methods were called
        await engine.Received(1).Deserialize(
            command.SpriggitPath,
            Arg.Any<FilePath>(),
            command.DataFolder,
            0u,
            null,
            null,
            null,
            Arg.Any<CancellationToken?>());
    
        await engine.Received(1).Serialize(
            Arg.Any<ModPath>(),
            command.SpriggitPath,
            command.DataFolder,
            false,
            true,
            null,
            Arg.Is<SpriggitMeta>(m => m.Source.Version == command.PackageVersion),
            Arg.Any<CancellationToken?>());
    
        // Verify the meta file was actually updated
        var metaPath = Path.Combine(existingsSpriggitPath, "spriggit-meta.json");
        var updatedMetaJson = _fileSystem.File.ReadAllText(metaPath);
        var updatedMeta = JsonConvert.DeserializeObject<SpriggitModKeyMetaSerialize>(updatedMetaJson, SpriggitExternalMetaPersister.JsonSettings);
        updatedMeta!.Version.ShouldBe(command.PackageVersion);
    }
    
    #region Git Operations Tests

    [Theory, MutagenAutoData]
    public async Task Execute_WithGitOperationsEnabled_ChecksForUncommittedChanges(
        ModKey modKey,
        [Frozen] ISpriggitEngine engine,
        UpgradeTargetSpriggitVersionRunner sut)
    {
        // Arrange
        var gitRepoPath = CreateGitRepository(withInitialCommit: true, withUncommittedChanges: false);
        var spriggitPath = Path.Combine(gitRepoPath, "spriggit-data");
        Directory.CreateDirectory(spriggitPath);

        var command = new UpgradeTargetSpriggitVersionCommand
        {
            SkipGitOperations = false,
            PackageVersion = "0.40.0",
            SpriggitPath = spriggitPath,
            DataFolder = Path.Combine(gitRepoPath, "Data")
        };

        var originalMeta = new SpriggitModKeyMeta(
            new SpriggitSource { PackageName = "Spriggit.Yaml.Starfield", Version = "0.39.11" },
            GameRelease.Starfield,
            modKey);

        var persister = new SpriggitExternalMetaPersister(_fileSystem);
        persister.Persist(command.SpriggitPath, originalMeta);

        // Stage the newly created meta file so it doesn't count as uncommitted changes
        using (var repo = new Repository(gitRepoPath))
        {
            var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
            Commands.Stage(repo, "spriggit-data/spriggit-meta.json");
            repo.Commit("Add spriggit meta file", signature, signature);
        }

        // Replace the GitOperations in the runner with our real instance
        var metaUpdater = new SpriggitMetaUpdater(_logger, _fileSystem, persister);
        var runnerWithRealGit = new UpgradeTargetSpriggitVersionRunner(
            engine,
            metaUpdater,
            _fileSystem,
            persister,
            _gitOperations,
            _logger);

        // Act
        var result = await runnerWithRealGit.Execute(command);

        // Assert
        result.ShouldBe(0);
        // Verify that git operations were actually performed by checking repo state
        var hasChanges = await _gitOperations.HasUncommittedChanges(command.SpriggitPath);
        // After successful upgrade, changes should be committed
        hasChanges.ShouldBeFalse();
    }

    [Theory, MutagenAutoData]
    public async Task Execute_WithUncommittedChanges_ReturnsErrorAndDoesNotProceed(
        ModKey modKey,
        [Frozen] ISpriggitEngine engine,
        UpgradeTargetSpriggitVersionRunner sut)
    {
        // Arrange
        var gitRepoPath = CreateGitRepository(withInitialCommit: true, withUncommittedChanges: true);
        var spriggitPath = Path.Combine(gitRepoPath, "spriggit-data");
        Directory.CreateDirectory(spriggitPath);

        var command = new UpgradeTargetSpriggitVersionCommand
        {
            SkipGitOperations = false,
            PackageVersion = "0.40.0",
            SpriggitPath = spriggitPath,
            DataFolder = Path.Combine(gitRepoPath, "Data")
        };

        var originalMeta = new SpriggitModKeyMeta(
            new SpriggitSource { PackageName = "Spriggit.Yaml.Starfield", Version = "0.39.11" },
            GameRelease.Starfield,
            modKey);

        var persister = new SpriggitExternalMetaPersister(_fileSystem);
        persister.Persist(command.SpriggitPath, originalMeta);

        var metaUpdater = new SpriggitMetaUpdater(_logger, _fileSystem, persister);
        var runnerWithRealGit = new UpgradeTargetSpriggitVersionRunner(
            engine,
            metaUpdater,
            _fileSystem,
            persister,
            _gitOperations,
            _logger);

        // Act
        var result = await runnerWithRealGit.Execute(command);

        // Assert
        result.ShouldBe(1); // Should return error code for uncommitted changes
        _logger.Received(1).Error("Git repository has uncommitted changes. Please commit or stash your changes before upgrading Spriggit version.");

        // Verify engine methods were never called
        await engine.DidNotReceive().Deserialize(Arg.Any<string>(), Arg.Any<FilePath>(), Arg.Any<DirectoryPath?>(), Arg.Any<uint>(), Arg.Any<bool?>(), Arg.Any<IEngineEntryPoint?>(), Arg.Any<SpriggitSource?>(), Arg.Any<CancellationToken?>());
        await engine.DidNotReceive().Serialize(Arg.Any<ModPath>(), Arg.Any<DirectoryPath>(), Arg.Any<DirectoryPath?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IEngineEntryPoint?>(), Arg.Any<SpriggitMeta?>(), Arg.Any<CancellationToken?>());
    }

    [Theory, MutagenAutoData]
    public async Task Execute_WithGitOperationsDisabled_SkipsGitChecksAndCommit(
        ModKey modKey,
        [Frozen] ISpriggitEngine engine,
        UpgradeTargetSpriggitVersionRunner sut)
    {
        // Arrange
        var gitRepoPath = CreateGitRepository(withInitialCommit: true, withUncommittedChanges: true);
        var spriggitPath = Path.Combine(gitRepoPath, "spriggit-data");
        Directory.CreateDirectory(spriggitPath);

        var command = new UpgradeTargetSpriggitVersionCommand
        {
            SkipGitOperations = true,
            PackageVersion = "0.40.0",
            SpriggitPath = spriggitPath,
            DataFolder = Path.Combine(gitRepoPath, "Data")
        };

        var originalMeta = new SpriggitModKeyMeta(
            new SpriggitSource { PackageName = "Spriggit.Yaml.Starfield", Version = "0.39.11" },
            GameRelease.Starfield,
            modKey);

        var persister = new SpriggitExternalMetaPersister(_fileSystem);
        persister.Persist(command.SpriggitPath, originalMeta);

        var metaUpdater = new SpriggitMetaUpdater(_logger, _fileSystem, persister);
        var runnerWithRealGit = new UpgradeTargetSpriggitVersionRunner(
            engine,
            metaUpdater,
            _fileSystem,
            persister,
            _gitOperations,
            _logger);

        // Verify there are uncommitted changes before the test
        var hasChangesBefore = await _gitOperations.HasUncommittedChanges(command.SpriggitPath);
        hasChangesBefore.ShouldBeTrue(); // Confirm our test setup

        // Act
        var result = await runnerWithRealGit.Execute(command);

        // Assert
        result.ShouldBe(0);

        // Verify uncommitted changes still exist (git operations were skipped)
        var hasChangesAfter = await _gitOperations.HasUncommittedChanges(command.SpriggitPath);
        hasChangesAfter.ShouldBeTrue(); // Changes should still be there since git ops were skipped

        // Verify upgrade still proceeded normally (git operations are skipped but engine operations still happen)
        await engine.Received(1).Deserialize(Arg.Any<string>(), Arg.Any<FilePath>(), Arg.Any<DirectoryPath?>(), Arg.Any<uint>(), Arg.Any<bool?>(), Arg.Any<IEngineEntryPoint?>(), Arg.Any<SpriggitSource?>(), Arg.Any<CancellationToken?>());
        await engine.Received(1).Serialize(Arg.Any<ModPath>(), Arg.Any<DirectoryPath>(), Arg.Any<DirectoryPath?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IEngineEntryPoint?>(), Arg.Any<SpriggitMeta?>(), Arg.Any<CancellationToken?>());
    }

    [Theory, MutagenAutoData]
    public async Task Execute_WithInvalidGitRepo_LogsWarningButStillSucceeds(
        ModKey modKey,
        [Frozen] ISpriggitEngine engine,
        UpgradeTargetSpriggitVersionRunner sut)
    {
        // Arrange - create an invalid git repo that will cause commit failures
        var invalidRepoPath = Path.Combine(_tempFolder.Dir, "invalid-repo");
        Directory.CreateDirectory(invalidRepoPath);

        // Create a .git directory but not a proper repo (will cause LibGit2Sharp to fail)
        var gitDir = Path.Combine(invalidRepoPath, ".git");
        Directory.CreateDirectory(gitDir);

        var spriggitPath = Path.Combine(invalidRepoPath, "spriggit-data");
        Directory.CreateDirectory(spriggitPath);

        var command = new UpgradeTargetSpriggitVersionCommand
        {
            SkipGitOperations = false,
            PackageVersion = "0.40.0",
            SpriggitPath = spriggitPath,
            DataFolder = Path.Combine(invalidRepoPath, "Data")
        };

        var originalMeta = new SpriggitModKeyMeta(
            new SpriggitSource { PackageName = "Spriggit.Yaml.Starfield", Version = "0.39.11" },
            GameRelease.Starfield,
            modKey);

        var persister = new SpriggitExternalMetaPersister(_fileSystem);
        persister.Persist(command.SpriggitPath, originalMeta);

        var metaUpdater = new SpriggitMetaUpdater(_logger, _fileSystem, persister);
        var runnerWithRealGit = new UpgradeTargetSpriggitVersionRunner(
            engine,
            metaUpdater,
            _fileSystem,
            persister,
            _gitOperations,
            _logger);

        // Act
        var result = await runnerWithRealGit.Execute(command);

        // Assert
        result.ShouldBe(0); // Should succeed despite git issues
        // Verify that engine operations still happened
        await engine.Received(1).Deserialize(Arg.Any<string>(), Arg.Any<FilePath>(), Arg.Any<DirectoryPath?>(), Arg.Any<uint>(), Arg.Any<bool?>(), Arg.Any<IEngineEntryPoint?>(), Arg.Any<SpriggitSource?>(), Arg.Any<CancellationToken?>());
        await engine.Received(1).Serialize(Arg.Any<ModPath>(), Arg.Any<DirectoryPath>(), Arg.Any<DirectoryPath?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<IEngineEntryPoint?>(), Arg.Any<SpriggitMeta?>(), Arg.Any<CancellationToken?>());
    }

    #endregion
}