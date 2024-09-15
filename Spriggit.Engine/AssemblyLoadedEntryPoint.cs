using System.IO.Abstractions;
using System.Reactive.Disposables;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using NuGet.Packaging.Core;
using Spriggit.Core;

namespace Spriggit.Engine;

public class AssemblyLoadedEntryPoint : IEngineEntryPoint
{
    private readonly CompositeDisposable _disposable;
    private readonly IEntryPoint _entryPoint;
    public PackageIdentity Package { get; }

    public AssemblyLoadedEntryPoint(
        IEntryPoint entryPoint,
        PackageIdentity package,
        CompositeDisposable disposable)
    {
        _disposable = disposable;
        _entryPoint = entryPoint;
        Package = package;
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
        await _entryPoint.Serialize(
            modPath, outputDir, dataPath, knownMasters, release, 
            workDropoff, fileSystem, streamCreator, meta, cancel);
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
        await _entryPoint.Deserialize(
            inputPath,
            outputPath,
            dataPath,
            knownMasters,
            workDropoff,
            fileSystem,
            streamCreator,
            cancel);
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}
