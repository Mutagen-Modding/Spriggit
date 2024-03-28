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

    public async Task Serialize(ModPath modPath, DirectoryPath outputDir, GameRelease release, IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, ICreateStream? streamCreator, SpriggitSource meta, CancellationToken cancel)
    {
        var serializeMethod = _wrappedEndPoint.GetType().GetMethods().FirstOrDefault(m => m.Name == "Serialize")!;
        var metaType = serializeMethod.GetParameters()[6];
        var metaReplacement = Activator.CreateInstance(metaType.ParameterType)!;
        dynamic dynamicMeta = metaReplacement;
        dynamicMeta.PackageName = meta.PackageName;
        dynamicMeta.Version = meta.Version;

        var ret = serializeMethod.Invoke(_wrappedEndPoint, new object?[]
        {
            modPath,
            outputDir,
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

    public async Task Deserialize(string inputPath, string outputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem,
        ICreateStream? streamCreator, CancellationToken cancel)
    {
        var deserializeMethod = _wrappedEndPoint.GetType().GetMethods().FirstOrDefault(m => m.Name == "Deserialize")!;
        
        var ret = deserializeMethod.Invoke(_wrappedEndPoint, new object?[]
        {
            inputPath,
            outputPath,
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

    public async Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(string inputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator,
        CancellationToken cancel)
    {
        dynamic dynamicRet = await ((dynamic)_wrappedEndPoint).TryGetMetaInfo(inputPath, workDropoff, fileSystem, streamCreator, cancel);
        return new SpriggitEmbeddedMeta(
            new SpriggitSource()
            {
                PackageName = dynamicRet.Source.PackageName,
                Version = dynamicRet.Source.Version
            },
            (GameRelease)(int)dynamicRet.Release,
            ModKey.FromNameAndExtension(dynamicRet.ModKey.ToString()));
    }
}