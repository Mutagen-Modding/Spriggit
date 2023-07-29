using System.IO.Abstractions;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Engine;

public class SpriggitEngine
{
    private readonly IFileSystem _fileSystem;
    private readonly IWorkDropoff? _workDropoff;
    private readonly ICreateStream? _createStream;
    private readonly EntryPointCache _entryPointCache;
    private readonly GetMetaToUse _getMetaToUse;

    public SpriggitEngine(
        IFileSystem fileSystem,
        IWorkDropoff? workDropoff,
        ICreateStream? createStream,
        EntryPointCache entryPointCache,
        GetMetaToUse getMetaToUse)
    {
        _fileSystem = fileSystem;
        _workDropoff = workDropoff;
        _createStream = createStream;
        _entryPointCache = entryPointCache;
        _getMetaToUse = getMetaToUse;
    }

    public async Task Serialize(
        FilePath bethesdaPluginPath, 
        DirectoryPath outputFolder, 
        SpriggitMeta meta,
        CancellationToken cancel)
    {
        var entryPt = await _entryPointCache.GetFor(meta, cancel);
        if (entryPt == null) throw new NotSupportedException($"Could not locate entry point for: {meta}");

        await entryPt.EntryPoint.Serialize(
            bethesdaPluginPath,
            outputFolder,
            meta.Release,
            fileSystem: _fileSystem,
            workDropoff: _workDropoff,
            streamCreator: _createStream,
            meta: new SpriggitSource()
            {
                PackageName = entryPt.Package.Id,
                Version = entryPt.Package.Version.ToString()
            },
            cancel: cancel);
    }

    public async Task Deserialize(
        string spriggitPluginPath, 
        FilePath outputFile,
        SpriggitSource? source,
        CancellationToken cancel)
    {
        var meta = await _getMetaToUse.Get(source, spriggitPluginPath, cancel);
        
        var entryPt = await _entryPointCache.GetFor(meta, cancel);
        if (entryPt == null) throw new NotSupportedException($"Could not locate entry point for: {meta}");

        cancel.ThrowIfCancellationRequested();
        
        await entryPt.EntryPoint.Deserialize(
            spriggitPluginPath,
            outputFile,
            workDropoff: _workDropoff,
            fileSystem: _fileSystem,
            streamCreator: _createStream,
            cancel: cancel);
    }
}