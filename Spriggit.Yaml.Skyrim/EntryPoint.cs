using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Serialization.Streams;
using Mutagen.Bethesda.Serialization.Yaml;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Serialization.Skyrim.Yaml;

public class EntryPoint : IEntryPoint<ISkyrimMod, ISkyrimModGetter>
{
    public async Task Serialize(
        ModPath modPath, 
        DirectoryPath dir,
        GameRelease release,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        SpriggitSource meta,
        CancellationToken cancel)
    {
        fileSystem = fileSystem.GetOrDefault();
        using var modGetter = SkyrimMod.CreateFromBinaryOverlay(modPath, release.ToSkyrimRelease(), fileSystem: fileSystem);
        await MutagenYamlConverter.Instance.Serialize(
            modGetter,
            dir,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: streamCreator,
            extraMeta: meta,
            cancel: cancel);
    }
 
    public async Task<ISkyrimMod> Deserialize(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        CancellationToken cancel)
    {
        return await MutagenYamlConverter.Instance.Deserialize(
            inputPath,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: streamCreator,
            cancel: cancel);
    }

    public Task<SpriggitMeta?> TryGetMetaInfo(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}