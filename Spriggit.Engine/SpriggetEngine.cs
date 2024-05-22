using System.IO.Abstractions;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Strings;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine;

public class SpriggitEngine(
    IFileSystem fileSystem,
    IWorkDropoff? workDropoff,
    ICreateStream? createStream,
    EntryPointCache entryPointCache,
    SpriggitMetaLocator spriggitMetaLocator,
    ILogger logger,
    GetMetaToUse getMetaToUse,
    SerializeBlocker serializeBlocker,
    CurrentVersionsProvider currentVersionsProvider,
    SpriggitEmbeddedMetaPersister metaPersister,
    PluginBackupCreator pluginBackupCreator)
{
    public async Task Serialize(
        ModPath bethesdaPluginPath, 
        DirectoryPath outputFolder, 
        SpriggitMeta? meta = default,
        CancellationToken? cancel = default)
    {
        cancel ??= CancellationToken.None;
        
        logger.Information("Spriggit version {Version}", currentVersionsProvider.SpriggitVersion);
        
        serializeBlocker.CheckAndBlock(outputFolder);
        
        if (meta == null)
        {
            meta = spriggitMetaLocator.LocateAndParse(outputFolder);
        }

        if (meta == null)
        {
            throw new NotSupportedException($"Could not locate meta to run with.  Either run serialize in with a .spriggit file present, or specify at least GameRelease and PackageName");
        }
        
        logger.Information("Getting entry point for {Meta}", meta);
        var entryPt = await entryPointCache.GetFor(meta, cancel.Value);
        if (entryPt == null) throw new NotSupportedException($"Could not locate entry point for: {meta}");

        var source = new SpriggitSource()
        {
            PackageName = entryPt.Package.Id,
            Version = entryPt.Package.Version.ToString()
        };
        
        logger.Information("Starting to serialize from {BethesdaPluginPath} to {Output} with {Meta}", bethesdaPluginPath, outputFolder, meta);
        await entryPt.Serialize(
            bethesdaPluginPath,
            outputFolder,
            meta.Release,
            fileSystem: fileSystem,
            workDropoff: workDropoff,
            streamCreator: createStream,
            meta: source,
            cancel: cancel.Value);
        metaPersister.Persist(
            outputFolder, 
            new SpriggitEmbeddedMeta(
                source,
                meta.Release,
                bethesdaPluginPath.ModKey));
        logger.Information("Finished serializing from {BethesdaPluginPath} to {Output} with {Meta}", bethesdaPluginPath, outputFolder, meta);
    }

    public async Task Deserialize(
        string spriggitPluginPath, 
        FilePath outputFile,
        uint backupDays,
        SpriggitSource? source = default,
        CancellationToken? cancel = default)
    {
        cancel ??= CancellationToken.None;
        
        logger.Information("Spriggit version {Version}", currentVersionsProvider.SpriggitVersion);
        
        logger.Information("Getting meta to use for {Source} at path {PluginPath}", source, spriggitPluginPath);
        var meta = await getMetaToUse.Get(source, spriggitPluginPath, cancel.Value);
        
        logger.Information("Getting entry point for {Meta}", meta);
        var entryPt = await entryPointCache.GetFor(meta, cancel.Value);
        if (entryPt == null) throw new NotSupportedException($"Could not locate entry point for: {meta}");

        cancel.Value.ThrowIfCancellationRequested();
        
        using var tmp = TempFolder.FactoryByAddedPath(Path.Combine("Spriggit", "Translations", Path.GetRandomFileName()));

        string tempOutput = Path.Combine(tmp.Dir, Path.GetFileName(outputFile));
        
        pluginBackupCreator.Backup(outputFile, backupDays: backupDays);

        logger.Information("Starting to deserialize from {BethesdaPluginPath} to temp output {Output} with {Meta}", 
            spriggitPluginPath, tempOutput, meta);
        await entryPt.Deserialize(
            spriggitPluginPath,
            tempOutput,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: createStream,
            cancel: cancel.Value);
        logger.Information("Finished deserializing with {BethesdaPluginPath} to temp output {Output} with {Meta}", 
            spriggitPluginPath, tempOutput, meta);
    
        var dir = outputFile.Directory;
        if (dir != null)
        {
            logger.Information("Creating output directory {Dir}", dir);
            fileSystem.Directory.CreateDirectory(dir);
        }

        logger.Information("Moving file to final output {Output}", outputFile);
        File.Move(tempOutput, outputFile, overwrite: true);
        logger.Information("Moved file to final output {Output}", outputFile);
    }
}