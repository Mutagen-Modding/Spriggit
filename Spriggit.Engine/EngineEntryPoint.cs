using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using NuGet.Packaging.Core;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine;

public interface IEngineEntryPoint : IEntryPoint, IDisposable
{
    PackageIdentity Package { get; }
}

public record EngineEntryPointWrapperItem(IEntryPoint? EntryPoint, ISimplisticEntryPoint? SimplisticEntryPoint);

public class EngineEntryPointWrapper : IEngineEntryPoint
{
    private readonly ILogger _logger;
    private readonly EngineEntryPointWrapperItem[] _entryPoints;
    public PackageIdentity Package { get; }

    public EngineEntryPointWrapper(
        ILogger logger,
        PackageIdentity packageIdentity,
        params EngineEntryPointWrapperItem[] entryPoints)
    {
        _logger = logger;
        _entryPoints = entryPoints;
        Package = packageIdentity;
    }

    public async Task Serialize(ModPath modPath, DirectoryPath outputDir, 
        DirectoryPath? dataPath, GameRelease release, IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, ICreateStream? streamCreator, SpriggitSource meta, CancellationToken cancel)
    {
        Exception? lastEx = null;
        foreach (var entryPt in _entryPoints)
        {
            try
            {
                if (entryPt.EntryPoint != null)
                {
                    await entryPt.EntryPoint.Serialize(
                        modPath, outputDir, dataPath, release, workDropoff,
                        fileSystem, streamCreator, meta, cancel);
                    return;
                }
                else if (entryPt.SimplisticEntryPoint != null)
                {
                    await entryPt.SimplisticEntryPoint.Serialize(
                        modPath, outputDir, dataPath, (int)release, 
                        meta.PackageName, meta.Version, cancel);
                    return;
                }
            }
            catch (Exception e)
            {
                lastEx = e;
                _logger.Warning(e, "Error serializing against entry point {Type} for identity {Identity}", entryPt.GetType(), Package);
            }
        }

        throw lastEx ?? new ExecutionEngineException("Unknown entry point error");
    }

    public async Task Deserialize(string inputPath, string outputPath, 
        DirectoryPath? dataPath,
        IWorkDropoff? workDropoff, IFileSystem? fileSystem,
        ICreateStream? streamCreator, CancellationToken cancel)
    {
        Exception? lastEx = null;
        foreach (var entryPt in _entryPoints)
        {
            try
            {
                if (entryPt.EntryPoint != null)
                {
                    await entryPt.EntryPoint.Deserialize(
                        inputPath, outputPath, dataPath, workDropoff,
                        fileSystem, streamCreator, cancel);
                    return;
                }
                else if (entryPt.SimplisticEntryPoint != null)
                {
                    await entryPt.SimplisticEntryPoint.Deserialize(
                        inputPath, outputPath, dataPath, cancel);
                    return;
                }
            }
            catch (Exception e)
            {
                lastEx = e;
                _logger.Warning(e, "Error deserializing against entry point {Type} for identity {Identity}", entryPt.GetType(), Package);
            }
        }

        throw lastEx ?? new ExecutionEngineException("Unknown entry point error");
    }

    public async Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(string inputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator,
        CancellationToken cancel)
    {
        Exception? lastEx = null;
        foreach (var entryPt in _entryPoints)
        {
            try
            {
                if (entryPt.EntryPoint != null)
                {
                    return await entryPt.EntryPoint.TryGetMetaInfo(
                        inputPath, workDropoff, fileSystem,
                        streamCreator, cancel);
                }
                else if (entryPt.SimplisticEntryPoint != null)
                {
                    return await entryPt.SimplisticEntryPoint.TryGetMetaInfo(
                        inputPath, cancel);
                }
            }
            catch (Exception e)
            {
                lastEx = e;
                _logger.Warning(e, "Error getting meta info against entry point {Type} for identity {Identity}", entryPt.GetType(), Package);
            }
        }

        throw lastEx ?? new ExecutionEngineException("Unknown entry point error");
    }

    public void Dispose()
    {
    }
}