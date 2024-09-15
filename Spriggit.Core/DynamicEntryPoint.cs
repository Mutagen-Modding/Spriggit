using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;

namespace Spriggit.Core;

public class DynamicEntryPoint : IEntryPoint
{
    private object _wrappedEndPoint;

    public DynamicEntryPoint(object wrappedEndPoint)
    {
        _wrappedEndPoint = wrappedEndPoint;
    }

    public async Task Serialize(
        ModPath modPath,
        DirectoryPath outputDir, 
        DirectoryPath? dataPath,
        KnownMaster[] knownMasters,
        GameRelease release,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator, 
        SpriggitSource meta, 
        CancellationToken cancel)
    {
        var serializeMethod = _wrappedEndPoint.GetType().GetMethods().FirstOrDefault(m => m.Name == "Serialize")!;
        var metaType = serializeMethod.GetParameters()[8];
        var metaReplacement = Activator.CreateInstance(metaType.ParameterType)!;
        dynamic dynamicMeta = metaReplacement;
        dynamicMeta.PackageName = meta.PackageName;
        dynamicMeta.Version = meta.Version;

        var ret = serializeMethod.Invoke(_wrappedEndPoint, new object?[]
        {
            modPath,
            outputDir,
            dataPath,
            knownMasters,
            release,
            workDropoff,
            fileSystem,
            streamCreator,
            metaReplacement,
            cancel
        });
        if (ret is Task t)
        {
            await t;
        }
    }

    public async Task Deserialize(
        string inputPath,
        string outputPath,
        DirectoryPath? dataPath,
        KnownMaster[] knownMasters,
        IWorkDropoff? workDropoff, 
        IFileSystem? fileSystem,
        ICreateStream? streamCreator, 
        CancellationToken cancel)
    {
        var deserializeMethod = _wrappedEndPoint.GetType().GetMethods().FirstOrDefault(m => m.Name == "Deserialize")!;
        
        var ret = deserializeMethod.Invoke(_wrappedEndPoint, new object?[]
        {
            inputPath,
            outputPath,
            dataPath,
            knownMasters,
            workDropoff,
            fileSystem,
            streamCreator,
            cancel
        });
        if (ret is Task t)
        {
            await t;
        }
    }

    public void Dispose()
    {
        if (_wrappedEndPoint is IDisposable disp)
        {
            disp.Dispose();
        }
    }
}