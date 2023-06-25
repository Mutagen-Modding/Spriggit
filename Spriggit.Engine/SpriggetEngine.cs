using System.IO.Abstractions;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Serialization.Streams;
using Noggog;
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
        SpriggitMeta meta)
    {
        var entryPt = await _entryPointCache.GetFor(meta);
        if (entryPt == null)
        {
            throw new NotSupportedException($"Could not locate entry point for: {meta}");
        }

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
            });
    }

    public async Task Deserialize(
        string spriggitPluginPath, 
        FilePath outputFile,
        SpriggitSource? source)
    {
        var meta = await _getMetaToUse.Get(source, spriggitPluginPath);
        
        var entryPt = await _entryPointCache.GetFor(meta);
        if (entryPt == null)
        {
            throw new NotSupportedException($"Could not locate entry point for: {meta}");
        }
        
        var mod = await entryPt.EntryPoint.Deserialize(
            spriggitPluginPath,
            workDropoff: _workDropoff,
            fileSystem: _fileSystem,
            streamCreator: _createStream);
        
        // ToDo
        // Pass in create stream?
        mod.WriteToBinaryParallel(outputFile, fileSystem: _fileSystem, param: new BinaryWriteParameters()
        {
            ModKey = ModKeyOption.CorrectToPath
        });
    }
}