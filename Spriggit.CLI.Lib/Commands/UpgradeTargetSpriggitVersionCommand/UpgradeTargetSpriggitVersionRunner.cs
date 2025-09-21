using System.IO.Abstractions;
using Noggog.IO;
using Serilog;
using Spriggit.Core;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.CLI.Lib.Commands.UpgradeTargetSpriggitVersionCommand;

public class UpgradeTargetSpriggitVersionRunner
{
    private readonly ISpriggitEngine _engine;
    private readonly SpriggitMetaUpdater _metaUpdater;
    private readonly IFileSystem _fileSystem;
    private readonly SpriggitExternalMetaPersister _metaPersister;
    private readonly ILogger _logger;

    public UpgradeTargetSpriggitVersionRunner(
        ISpriggitEngine engine,
        SpriggitMetaUpdater metaUpdater,
        IFileSystem fileSystem,
        SpriggitExternalMetaPersister metaPersister,
        ILogger logger)
    {
        _engine = engine;
        _metaUpdater = metaUpdater;
        _fileSystem = fileSystem;
        _metaPersister = metaPersister;
        _logger = logger;
    }

    private static UpgradeTargetSpriggitVersionRunnerContainer GetContainer(DebugState debugState)
    {
        return new UpgradeTargetSpriggitVersionRunnerContainer(new FileSystem(), debugState, LoggerSetup.Logger);
    }

    public static async Task<int> Run(UpgradeTargetSpriggitVersionCommand command)
    {
        LoggerSetup.Logger.Information("Command to upgrade target spriggit version");

        var container = GetContainer(new DebugState { ClearNugetSources = false });
        using var runnerResolver = container.Resolve();
        var runner = runnerResolver.Value;

        return await runner.Execute(command);
    }

    public async Task<int> Execute(UpgradeTargetSpriggitVersionCommand command)
    {
        var originalMeta = _metaPersister.TryParseEmbeddedMeta(command.SpriggitPath);
        if (originalMeta == null)
        {
            _logger.Error("Could not parse existing spriggit-meta.json in {SpriggitPath}", command.SpriggitPath);
            return 1;
        }

        // Deserialize the mod files first
        _logger.Information("Deserializing mod files from {SpriggitPath}", command.SpriggitPath);

        using var tmp = TempFolder.Factory(fileSystem: _fileSystem);
        var modPath = Path.Combine(tmp.Dir, originalMeta.ModKey.FileName);
        await _engine.Deserialize(
            spriggitPluginPath: command.SpriggitPath,
            outputFile: modPath,
            dataPath: command.DataFolder,
            entryPt: null,
            source: null,
            backupDays: 0,
            localize: null,
            cancel: CancellationToken.None);

        // Update the spriggit-meta.json file if a specific version was provided
        if (!string.IsNullOrEmpty(command.PackageVersion))
        {
            _logger.Information("Updating spriggit-meta.json to version {Version}", command.PackageVersion);

            if (!_metaUpdater.UpdateMetaVersion(command.SpriggitPath, command.PackageVersion))
            {
                _logger.Error("Failed to update spriggit-meta.json version");
                return 1;
            }
        }
        else
        {
            _logger.Information("No specific version provided, will upgrade to latest during serialization");
        }

        // Serialize the mod files back with the new version
        _logger.Information("Re-serializing mod files with new version");

        // Create updated metadata that preserves the original ModKey
        var updatedSource = new SpriggitSource()
        {
            PackageName = originalMeta.Source.PackageName,
            Version = !string.IsNullOrEmpty(command.PackageVersion)
                ? command.PackageVersion
                : originalMeta.Source.Version
        };

        var updatedMeta = new SpriggitMeta(
            updatedSource,
            originalMeta.Release);

        await _engine.Serialize(
            bethesdaPluginPath: modPath,
            outputFolder: command.SpriggitPath,
            dataPath: command.DataFolder,
            entryPt: null,
            postSerializeChecks: false,
            meta: updatedMeta,
            throwOnUnknown: true,
            cancel: CancellationToken.None);

        _logger.Information("Successfully upgraded spriggit version and re-serialized mod files");
        return 0;
    }
}