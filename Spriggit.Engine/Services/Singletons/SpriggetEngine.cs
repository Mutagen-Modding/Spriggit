using System.IO.Abstractions;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.IO.DI;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

public class SpriggitEngine(
    IFileSystem fileSystem,
    IWorkDropoff? workDropoff,
    ICreateStream? createStream,
    IEntryPointCache entryPointCache,
    SpriggitMetaLocator spriggitMetaLocator,
    ILogger logger,
    GetMetaToUse getMetaToUse,
    SerializeBlocker serializeBlocker,
    SpriggitExternalMetaPersister metaPersister,
    PluginBackupCreator pluginBackupCreator,
    IModFilesMover modFilesMover,
    LocalizeEnforcer localizeEnforcer,
    PostSerializeChecker postSerializeChecker,
    DataPathChecker dataPathChecker)
{
    public async Task Serialize(
        ModPath bethesdaPluginPath, 
        DirectoryPath outputFolder, 
        DirectoryPath? dataPath,
        bool postSerializeChecks,
        IEngineEntryPoint? entryPt = default,
        SpriggitMeta? meta = default,
        CancellationToken? cancel = default)
    {
        cancel ??= CancellationToken.None;
        
        serializeBlocker.CheckAndBlock(outputFolder);
        
        if (meta == null)
        {
            meta = spriggitMetaLocator.LocateAndParse(outputFolder);
        }

        if (meta == null)
        {
            throw new NotSupportedException($"Could not locate meta to run with.  Either run serialize in with a {SpriggitMetaLocator.ConfigFileName} file present, or specify at least GameRelease and PackageName");
        }
        
        dataPathChecker.CheckDataPath(meta.Release, dataPath);

        if (entryPt == null)
        {
            logger.Information("Getting entry point for {Meta}", meta);
            entryPt = await entryPointCache.GetFor(meta, cancel.Value);
            if (entryPt == null) throw new NotSupportedException($"Could not locate entry point for: {meta}");
        }

        var source = new SpriggitSource()
        {
            PackageName = entryPt.Package.Id,
            Version = entryPt.Package.Version.ToString()
        };
        
        logger.Information("Starting to serialize from {BethesdaPluginPath} to {Output} with {Meta}", bethesdaPluginPath, outputFolder, meta);
        await entryPt.Serialize(
            bethesdaPluginPath,
            outputDir: outputFolder,
            dataPath: dataPath,
            release: meta.Release,
            fileSystem: fileSystem,
            workDropoff: workDropoff,
            streamCreator: createStream,
            meta: source,
            cancel: cancel.Value);
        metaPersister.Persist(
            outputFolder, 
            new SpriggitModKeyMeta(
                source,
                meta.Release,
                bethesdaPluginPath.ModKey));
        if (postSerializeChecks)
        {
            await postSerializeChecker.Check(
                mod: bethesdaPluginPath,
                release: meta.Release,
                spriggit: outputFolder,
                dataPath: dataPath,
                entryPt: entryPt,
                cancel: cancel.Value);   
        }
        logger.Information("Finished serializing from {BethesdaPluginPath} to {Output} with {Meta}", bethesdaPluginPath, outputFolder, meta);
    }

    public async Task Deserialize(
        string spriggitPluginPath, 
        FilePath outputFile,
        DirectoryPath? dataPath,
        uint backupDays,
        bool? localize,
        IEngineEntryPoint? entryPt = default,
        SpriggitSource? source = default,
        CancellationToken? cancel = default)
    {
        cancel ??= CancellationToken.None;
        
        logger.Information("Getting meta to use for {Source} at path {PluginPath}", source, spriggitPluginPath);
        var meta = await getMetaToUse.Get(source, spriggitPluginPath, cancel.Value);

        if (entryPt == null)
        {
            logger.Information("Getting entry point for {Meta}", meta);
            entryPt = await entryPointCache.GetFor(meta.ToMeta(), cancel.Value);
            if (entryPt == null) throw new NotSupportedException($"Could not locate entry point for: {meta}");
        }

        cancel.Value.ThrowIfCancellationRequested();
        
        using var tmp = TempFolder.FactoryByAddedPath(Path.Combine("Spriggit", "Translations", Path.GetRandomFileName()));

        ModPath tempOutput = Path.Combine(tmp.Dir, Path.GetFileName(outputFile));
        
        pluginBackupCreator.Backup(outputFile, backupDays: backupDays);

        logger.Information("Starting to deserialize from {BethesdaPluginPath} to temp output {Output} with {Meta}", 
            spriggitPluginPath, tempOutput, meta);
        await entryPt.Deserialize(
            spriggitPluginPath,
            outputPath: tempOutput,
            dataPath: dataPath,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: createStream,
            cancel: cancel.Value);
        logger.Information("Finished deserializing with {BethesdaPluginPath} to temp output {Output} with {Meta}", 
            spriggitPluginPath, tempOutput, meta);

        if (localize != null)
        {
            localizeEnforcer.Localize(localize.Value, tempOutput, meta.Release);
        }
    
        var dir = outputFile.Directory;
        if (dir != null)
        {
            logger.Information("Creating output directory {Dir}", dir);
            fileSystem.Directory.CreateDirectory(dir);
        }

        logger.Information("Moving file to final output {Output}", outputFile);
        modFilesMover.MoveModTo(tempOutput, outputFile.Directory!.Value, overwrite: true);
        logger.Information("Moved file to final output {Output}", outputFile);
    }
}