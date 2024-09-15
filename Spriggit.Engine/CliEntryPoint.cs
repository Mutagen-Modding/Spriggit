using System.Diagnostics;
using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.Processes.DI;
using Noggog.WorkEngine;
using NuGet.Packaging.Core;
using Serilog;
using Spriggit.Core;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.Engine;

public class CliEntryPoint : IEntryPoint
{
    private readonly ILogger _logger;
    private readonly ProcessFactory _processFactory;
    private readonly FilePath _pathToExe;
    private readonly PackageVersioningChecker _packageVersioningChecker;
    private readonly PackageIdentity _package;

    public CliEntryPoint(
        ILogger logger,
        ProcessFactory processFactory,
        FilePath pathToExe,
        PackageVersioningChecker packageVersioningChecker,
        PackageIdentity package)
    {
        _logger = logger;
        _processFactory = processFactory;
        _pathToExe = pathToExe;
        _packageVersioningChecker = packageVersioningChecker;
        _package = package;
    }

    private string GetDataPathParam(string? dataPath)
    {
        if (dataPath != null
            && _packageVersioningChecker.SupportsDataPath(_package))
        {
            return $"-d \"{dataPath}\"";
        }

        return string.Empty;
    }
    
    public async Task Serialize(
        ModPath modPath, DirectoryPath outputDir, DirectoryPath? dataPath,
        KnownMaster[] knownMasters,
        GameRelease release,
        IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator, SpriggitSource meta,
        CancellationToken cancel)
    {
        var args = $"serialize -i \"{modPath.Path.Path}\" -o \"{outputDir.Path}\" -g {release} -p {_package.Id} -v {_package.Version.ToString().TrimEnd(".0")}{GetDataPathParam(dataPath)}";
        _logger.Information("Running CLI Entry point serialize with Args: {Args}", args);
        using var processWrapper = _processFactory.Create(
            new ProcessStartInfo(_pathToExe)
            {
                Arguments = args
            });
        using var outputSub = processWrapper.Output
            .Subscribe(x =>
            {
                _logger.Information(x);
            });
        using var err = processWrapper.Error
            .Subscribe(x =>
            {
                _logger.Error(x);
            });
        await processWrapper.Run();
    }

    public async Task Deserialize(string inputPath, string outputPath, DirectoryPath? dataPath, 
        KnownMaster[] knownMasters,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, ICreateStream? streamCreator, CancellationToken cancel)
    {
        var args = $"deserialize -i \"{inputPath}\" -o \"{outputPath}\" -p {_package.Id} -v {_package.Version.ToString().TrimEnd(".0")}{GetDataPathParam(dataPath)}";
        _logger.Information("Running CLI Entry point deserialize with Args: {Args}", args);
        using var processWrapper = _processFactory.Create(
            new ProcessStartInfo(_pathToExe)
            {
                Arguments = args
            });
        using var outputSub = processWrapper.Output
            .Subscribe(x =>
            {
                _logger.Information(x);
            });
        using var err = processWrapper.Error
            .Subscribe(x =>
            {
                _logger.Error(x);
            });
        await processWrapper.Run();
    }
}