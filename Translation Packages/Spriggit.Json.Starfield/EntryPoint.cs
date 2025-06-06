using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Serialization.Newtonsoft;
using Mutagen.Bethesda.Serialization.Utility;
using Mutagen.Bethesda.Starfield;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Core;
using Spriggit.TranslationPackages;

namespace Spriggit.Json.Starfield;

public class EntryPoint : IEntryPoint
{
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
        bool throwOnUnknown,
        CancellationToken cancel)
    {
        fileSystem = fileSystem.GetOrDefault();
        using var modGetter = StarfieldMod
            .Create(release.ToStarfieldRelease())
            .FromPath(modPath)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataPath)
            .WithFileSystem(fileSystem)
            .WithKnownMasters(
                knownMasters.Select(x => new KeyedMasterStyle(x.ModKey, x.Style))
                    .ToArray())
            .ThrowIfUnknownSubrecord(shouldThrow: throwOnUnknown)
            .Construct();
        await MutagenJsonConverter.Instance.Serialize(
            modGetter,
            outputDir,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: streamCreator,
            extraMeta: meta,
            cancel: cancel);
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
        var mod = await MutagenJsonConverter.Instance.Deserialize(
            inputPath,
            workDropoff: workDropoff,
            fileSystem: fileSystem,
            streamCreator: streamCreator,
            cancel: cancel);
        await mod.BeginWrite
            .ToPath(outputPath)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataPath)
            .WithFileSystem(fileSystem)
            .WithKnownMasters(
                knownMasters.Select(x => new KeyedMasterStyle(x.ModKey, x.Style))
                    .ToArray())
            .AddNonOpinionatedWriteOptions()
            .WriteAsync();
    }

    private static readonly Mutagen.Bethesda.Serialization.Newtonsoft.NewtonsoftJsonSerializationReaderKernel ReaderKernel = new();
}