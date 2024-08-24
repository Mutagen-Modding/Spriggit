using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Serialization.Newtonsoft;
using Mutagen.Bethesda.Serialization.Utility;
using Mutagen.Bethesda.Oblivion;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Json.Oblivion;

public class EntryPoint : IEntryPoint, ISimplisticEntryPoint
{
    public async Task Serialize(
        ModPath modPath, 
        DirectoryPath outputDir,
        DirectoryPath? dataPath,
        GameRelease release,
        IWorkDropoff? workDropoff, 
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        SpriggitSource meta,
        CancellationToken cancel)
    {
        fileSystem = fileSystem.GetOrDefault();
        using var modGetter = OblivionMod
            .Create
            .FromPath(modPath)
            .WithDataFolder(dataPath)
            .WithFileSystem(fileSystem)
            .ThrowIfUnknownSubrecord()
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
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataPath)
            .ToPath(outputPath)
            .WithFileSystem(fileSystem)
            .WithRecordCount(RecordCountOption.Iterate)
            .WithModKeySync(ModKeyOption.CorrectToPath)
            .WithMastersListContent(MastersListContentOption.NoCheck)
            .WithMastersListOrdering(MastersListOrderingOption.NoCheck)
            .NoFormIDUniquenessCheck()
            .NoFormIDCompactnessCheck()
            .NoCheckIfLowerRangeDisallowed()
            .NoNullFormIDStandardization()
            .WriteAsync();
    }

    private static readonly Mutagen.Bethesda.Serialization.Newtonsoft.NewtonsoftJsonSerializationReaderKernel ReaderKernel = new();
    
    public async Task Serialize(
        string modPath, string outputDir, string? dataPath, 
        int release, string packageName, string version,
        CancellationToken cancel)
    {
        await Serialize(
            modPath: new ModPath(modPath),
            outputDir: outputDir,
            dataPath: dataPath,
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

    public Task Deserialize(string inputPath, string outputPath, string? dataPath, CancellationToken cancel)
    {
        return Deserialize(
            inputPath: inputPath,
            outputPath: outputPath,
            dataPath: dataPath,
            workDropoff: null,
            fileSystem: null,
            streamCreator: null,
            cancel: cancel);
    }
}