using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Serialization.Newtonsoft;
using Mutagen.Bethesda.Serialization.Streams;
using Mutagen.Bethesda.Serialization.Utility;
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

    private readonly static Mutagen.Bethesda.Serialization.Newtonsoft.NewtonsoftJsonSerializationReaderKernel ReaderKernel = new();
    
    public async Task<SpriggitMeta?> TryGetMetaInfo(
        string inputPath, IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, ICreateStream? streamCreator)
    {
        // ToDo
        // Serialization should generate this
        
        fileSystem = fileSystem.GetOrDefault();
        if (streamCreator == null || streamCreator is NoPreferenceStreamCreator)
        {
            streamCreator = NormalFileStreamCreator.Instance;
        }
        SpriggitSource src = new();
        SerializationHelper.ExtractMeta(
            fileSystem: fileSystem,
            modKeyPath: inputPath,
            path: Path.Combine(inputPath, SerializationHelper.RecordDataFileName(ReaderKernel.ExpectedExtension)),
            streamCreator: streamCreator,
            kernel: ReaderKernel,
            extraMeta: src,
            metaReader: static (r, m, k, s) => Spriggit.Core.SpriggitSource_Serialization.DeserializeInto(r, k, m, s),
            modKey: out var modKey,
            release: out var release);

        return new SpriggitMeta(src, release);
    }
}