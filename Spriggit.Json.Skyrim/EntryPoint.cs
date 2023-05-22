using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Serialization.Newtonsoft;
using Mutagen.Bethesda.Serialization.Streams;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Serialization.Skyrim.Json;

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
        await MutagenJsonConverter.Instance.Serialize(
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
        return await MutagenJsonConverter.Instance.Deserialize(
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