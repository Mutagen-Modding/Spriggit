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

public class EngineEntryPointWrapper : IEngineEntryPoint
{
    private readonly ILogger _logger;
    private readonly IEntryPoint[] _entryPoints;
    public PackageIdentity Package { get; }

    public EngineEntryPointWrapper(
        ILogger logger,
        PackageIdentity packageIdentity,
        params IEntryPoint[] entryPoints)
    {
        _logger = logger;
        _entryPoints = entryPoints;
        Package = packageIdentity;
    }

    public Task Serialize(ModPath modPath, DirectoryPath outputDir, GameRelease release, IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, ICreateStream? streamCreator, SpriggitSource meta, CancellationToken cancel)
    {
        Exception? lastEx = null;
        foreach (var entryPt in _entryPoints)
        {
            try
            {
                return entryPt.Serialize(modPath, outputDir, release, workDropoff, fileSystem, streamCreator, meta, cancel);
            }
            catch (Exception e)
            {
                lastEx = e;
                _logger.Warning(e, "Error serializing against entry point {Type} for identity {Identity}", entryPt.GetType(), Package);
            }
        }

        throw lastEx ?? new ExecutionEngineException("Unknown entry point error");
    }

    public Task Deserialize(string inputPath, string outputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem,
        ICreateStream? streamCreator, CancellationToken cancel)
    {
        Exception? lastEx = null;
        foreach (var entryPt in _entryPoints)
        {
            try
            {
                return entryPt.Deserialize(inputPath, outputPath, workDropoff, fileSystem, streamCreator, cancel);
            }
            catch (Exception e)
            {
                lastEx = e;
                _logger.Warning(e, "Error deserializing against entry point {Type} for identity {Identity}", entryPt.GetType(), Package);
            }
        }

        throw lastEx ?? new ExecutionEngineException("Unknown entry point error");
    }

    public Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(string inputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator,
        CancellationToken cancel)
    {
        Exception? lastEx = null;
        foreach (var entryPt in _entryPoints)
        {
            try
            {
                return entryPt.TryGetMetaInfo(inputPath, workDropoff, fileSystem, streamCreator, cancel);
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