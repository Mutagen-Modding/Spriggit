using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Serialization.Yaml;
using Mutagen.Bethesda.Serialization.Utility;
using Mutagen.Bethesda.Oblivion;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Yaml.Oblivion;

public class EntryPoint : IEntryPoint, ISimplisticEntryPoint
{
    protected static readonly BinaryWriteParameters NoCheckWriteParameters = new()
    {
        ModKey = ModKeyOption.CorrectToPath,
        MastersListContent = MastersListContentOption.NoCheck,
        RecordCount = RecordCountOption.NoCheck,
        MastersListOrdering = MastersListOrderingOption.NoCheck,
        NextFormID = NextFormIDOption.NoCheck,
        FormIDUniqueness = FormIDUniquenessOption.NoCheck,
        MasterFlag = MasterFlagOption.NoCheck,
        LightLimit = LightLimitOption.NoCheck,
        CleanNulls = false
    };

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
        using var modGetter = OblivionMod.CreateFromBinaryOverlay(
            modPath, 
            fileSystem: fileSystem,
            throwOnUnknownSubrecord: true);
        await MutagenYamlConverter.Instance.Serialize(
            modGetter,
            dir,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: streamCreator,
            extraMeta: meta,
            cancel: cancel);
    }

    public async Task Deserialize(
        string inputPath,
        string outputPath,
        IWorkDropoff? workDropoff, 
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        CancellationToken cancel)
    {
        var mod = await MutagenYamlConverter.Instance.Deserialize(
            inputPath,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: streamCreator,
            cancel: cancel);
        mod.WriteToBinaryParallel(outputPath, fileSystem: fileSystem, param: NoCheckWriteParameters);
    }

    private static readonly Mutagen.Bethesda.Serialization.Yaml.YamlSerializationReaderKernel ReaderKernel = new();
    
    public async Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(
        string inputPath, IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, ICreateStream? streamCreator,
        CancellationToken cancel)
    {
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
            release: out var release,
            cancel: cancel);

        return new SpriggitEmbeddedMeta(src, release, modKey);
    }

    public async Task Serialize(string modPath, string outputDir, int release, string packageName, string version,
        CancellationToken cancel)
    {
        await Serialize(
            modPath: new ModPath(modPath),
            dir: outputDir,
            release: (GameRelease)release,
            workDropoff: null,
            fileSystem: null,
            streamCreator: null,
            meta: new SpriggitSource()
            {
                PackageName = packageName,
                Version = version
            },
            cancel: cancel);
    }

    public Task Deserialize(string inputPath, string outputPath, CancellationToken cancel)
    {
        return Deserialize(
            inputPath: inputPath,
            outputPath: outputPath,
            workDropoff: null,
            fileSystem: null,
            streamCreator: null,
            cancel: cancel);
    }

    public Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(string inputPath, CancellationToken cancel)
    {
        return TryGetMetaInfo(
            inputPath, 
            workDropoff: null, 
            fileSystem: null, 
            streamCreator: null, 
            cancel: cancel);
    }
}