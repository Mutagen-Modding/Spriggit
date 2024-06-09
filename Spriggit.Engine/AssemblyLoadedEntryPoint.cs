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
    private readonly ISimplisticEntryPoint? _simplisticEntryPoint;
    private readonly IEntryPoint? _entryPoint;
    public PackageIdentity Package { get; }

    public AssemblyLoadedEntryPoint(
        IEntryPoint? entryPoint,
        ISimplisticEntryPoint? simplisticEntryPoint,
        PackageIdentity package,
        CompositeDisposable disposable)
    {
        _disposable = disposable;
        _simplisticEntryPoint = simplisticEntryPoint;
        _entryPoint = entryPoint;
        Package = package;
    }

    public async Task Serialize(ModPath modPath, DirectoryPath outputDir, GameRelease release, IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, ICreateStream? streamCreator, SpriggitSource meta, CancellationToken cancel)
    {
        if (_entryPoint != null)
        {
            await _entryPoint.Serialize(modPath, outputDir, release, workDropoff, fileSystem, streamCreator, meta, cancel);
        }
        else if (_simplisticEntryPoint != null)
        {
            await _simplisticEntryPoint.Serialize(
                modPath: modPath,
                outputDir: outputDir,
                release: (int)release,
                packageName: meta.PackageName,
                version: meta.Version,
                cancel);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public async Task Deserialize(string inputPath, string outputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem,
        ICreateStream? streamCreator, CancellationToken cancel)
    {
        if (_entryPoint != null)
        {
            try
            {
                await _entryPoint.Deserialize(inputPath, outputPath, workDropoff, fileSystem, streamCreator, cancel);
                return;
            }
            catch (ArgumentException)
            {
            }
        }

        if (_simplisticEntryPoint != null)
        {
            await _simplisticEntryPoint.Deserialize(inputPath: inputPath, outputPath: outputPath, cancel: cancel);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public async Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(string inputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator,
        CancellationToken cancel)
    {
        if (_entryPoint != null)
        {
            return await _entryPoint.TryGetMetaInfo(inputPath, workDropoff, fileSystem, streamCreator, cancel);
        }
        else if (_simplisticEntryPoint != null)
        {
            return await _simplisticEntryPoint.TryGetMetaInfo(inputPath: inputPath, cancel: cancel);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}
