using System.IO.Abstractions;
using Noggog;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

public class SpriggitMetaUpdater
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly SpriggitExternalMetaPersister _metaPersister;

    public SpriggitMetaUpdater(
        ILogger logger,
        IFileSystem fileSystem,
        SpriggitExternalMetaPersister metaPersister)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _metaPersister = metaPersister;
    }

    public bool UpdateMetaVersion(DirectoryPath spriggitFolder, string newVersion)
    {
        _logger.Information("Updating spriggit-meta.json version to {NewVersion} in {SpriggitFolder}", newVersion, spriggitFolder);

        var existingMeta = _metaPersister.TryParseEmbeddedMeta(spriggitFolder);
        if (existingMeta == null)
        {
            _logger.Error("Could not parse existing spriggit-meta.json in {SpriggitFolder}", spriggitFolder);
            return false;
        }

        var updatedSource = new SpriggitSource()
        {
            PackageName = existingMeta.Source.PackageName,
            Version = newVersion
        };

        var updatedMeta = new SpriggitModKeyMeta(
            updatedSource,
            existingMeta.Release,
            existingMeta.ModKey);

        _metaPersister.Persist(spriggitFolder, updatedMeta);

        _logger.Information("Successfully updated spriggit-meta.json version from {OldVersion} to {NewVersion}",
            existingMeta.Source.Version, newVersion);

        return true;
    }
}