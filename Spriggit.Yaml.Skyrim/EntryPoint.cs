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
        SpriggitSource meta)
    {
        using var modGetter = SkyrimMod.CreateFromBinaryOverlay(modPath, release.ToSkyrimRelease());
        await MutagenYamlConverter.Instance.Serialize(
            modGetter,
            dir,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: streamCreator,
            extraMeta: meta);
    }
 
    public async Task<ISkyrimMod> Deserialize(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator)
    {
        return await MutagenYamlConverter.Instance.Deserialize(
            inputPath,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: streamCreator);
    }

    public Task<SpriggitMeta?> TryGetMetaInfo(string inputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator)
    {
        throw new NotImplementedException();
    }
}