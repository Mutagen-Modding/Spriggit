using System.IO.Abstractions;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Engine;

public class SpriggitEngine(
    IFileSystem fileSystem,
    IWorkDropoff? workDropoff,
    ICreateStream? createStream,
    EntryPointCache entryPointCache,
    GetMetaToUse getMetaToUse)
{
    public async Task Serialize(
        FilePath bethesdaPluginPath, 
        DirectoryPath outputFolder, 
        SpriggitMeta meta,
        CancellationToken cancel)
    {
        var entryPt = await entryPointCache.GetFor(meta, cancel);
        if (entryPt == null) throw new NotSupportedException($"Could not locate entry point for: {meta}");

        await entryPt.EntryPoint.Serialize(
            bethesdaPluginPath,
            outputFolder,
            meta.Release,
            fileSystem: fileSystem,
            workDropoff: workDropoff,
            streamCreator: createStream,
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
        var meta = await getMetaToUse.Get(source, spriggitPluginPath, cancel);
        
        var entryPt = await entryPointCache.GetFor(meta, cancel);
        if (entryPt == null) throw new NotSupportedException($"Could not locate entry point for: {meta}");

        cancel.ThrowIfCancellationRequested();
        
        await entryPt.EntryPoint.Deserialize(
            spriggitPluginPath,
            outputFile,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: createStream,
            cancel: cancel);
    }
}