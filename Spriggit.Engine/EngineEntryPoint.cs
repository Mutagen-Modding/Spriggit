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

    public async Task Serialize(ModPath modPath, DirectoryPath outputDir, 
        DirectoryPath? dataPath, 
        KnownMaster[] knownMasters,
        GameRelease release, IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, ICreateStream? streamCreator, SpriggitSource meta, CancellationToken cancel)
    {
        Exception? lastEx = null;
        foreach (var entryPt in _entryPoints)
        {
            try
            {
                await entryPt.Serialize(
                    modPath, outputDir, dataPath, knownMasters, release, workDropoff,
                    fileSystem, streamCreator, meta, cancel);
                return;
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
        KnownMaster[] knownMasters,
        IWorkDropoff? workDropoff, IFileSystem? fileSystem,
        ICreateStream? streamCreator, CancellationToken cancel)
    {
        Exception? lastEx = null;
        foreach (var entryPt in _entryPoints)
        {
            try
            {
                await entryPt.Deserialize(
                    inputPath, outputPath, dataPath, knownMasters, workDropoff,
                    fileSystem, streamCreator, cancel);
                return;
            }
            catch (Exception e)
            {
                lastEx = e;
                _logger.Warning(e, "Error deserializing against entry point {Type} for identity {Identity}", entryPt.GetType(), Package);
            }
        }

        throw lastEx ?? new ExecutionEngineException("Unknown entry point error");
    }

    public void Dispose()
    {
    }
}