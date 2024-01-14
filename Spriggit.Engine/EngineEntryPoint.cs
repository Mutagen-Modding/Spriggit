using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using NuGet.Packaging.Core;
using Spriggit.Core;

namespace Spriggit.Engine;

public class EngineEntryPoint : IEntryPoint
{
    private readonly IEntryPoint _entryPoint;
    public PackageIdentity Package { get; }

    public EngineEntryPoint(
        IEntryPoint entryPoint,
        PackageIdentity package)
    {
        _entryPoint = entryPoint;
        Package = package;
    }

    public Task Serialize(ModPath modPath, DirectoryPath outputDir, GameRelease release, IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, ICreateStream? streamCreator, SpriggitSource meta, CancellationToken cancel)
    {
        return _entryPoint.Serialize(modPath, outputDir, release, workDropoff, fileSystem, streamCreator, meta, cancel);
    }

    public Task Deserialize(string inputPath, string outputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem,
        ICreateStream? streamCreator, CancellationToken cancel)
    {
        return _entryPoint.Deserialize(inputPath, outputPath, workDropoff, fileSystem, streamCreator, cancel);
    }

    public Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(string inputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator,
        CancellationToken cancel)
    {
        return _entryPoint.TryGetMetaInfo(inputPath, workDropoff, fileSystem, streamCreator, cancel);
    }
}