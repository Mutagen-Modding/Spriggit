using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Serialization.Newtonsoft;
using Mutagen.Bethesda.Fallout4;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Json.Fallout4;

public class EntryPoint : IEntryPoint, ISimplisticEntryPoint
{
    public async Task Serialize(
        ModPath modPath, 
        DirectoryPath outputPath,
        DirectoryPath? dataPath,
        GameRelease release,
        IWorkDropoff? workDropoff, 
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        SpriggitSource meta,
        CancellationToken cancel)
    {
        fileSystem = fileSystem.GetOrDefault();
        using var modGetter = Fallout4Mod
            .Create(release.ToFallout4Release())
            .FromPath(modPath)
            .WithDataFolder(dataPath)
            .WithFileSystem(fileSystem)
            .ThrowIfUnknownSubrecord()
            .Construct();
        await MutagenJsonConverter.Instance.Serialize(
            modGetter,
            outputPath,
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
    
    public async Task Serialize(string modPath, string outputPath, string? dataPath, int release, string packageName, string version,
        CancellationToken cancel)
    {
        await Serialize(
            modPath: new ModPath(modPath),
            outputPath: outputPath,
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