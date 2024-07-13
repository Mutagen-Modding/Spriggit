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

namespace Spriggit.Engine;

public class CliEntryPoint : ISimplisticEntryPoint
{
    private readonly ILogger _logger;
    private readonly ProcessFactory _processFactory;
    private readonly FilePath _pathToExe;
    private readonly PackageIdentity _package;

    public CliEntryPoint(
        ILogger logger,
        ProcessFactory processFactory,
        FilePath pathToExe,
        PackageIdentity package)
    {
        _logger = logger;
        _processFactory = processFactory;
        _pathToExe = pathToExe;
        _package = package;
    }
    
    public async Task Serialize(string modPath, string outputDir, int release, string packageName, string version,
        CancellationToken cancel)
    {
        var args = $"serialize -i \"{modPath}\" -o \"{outputDir}\" -g {release} -p {packageName} -v {version}";
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

    public async Task Deserialize(string inputPath, string outputPath, CancellationToken cancel)
    {
        var args = $"deserialize -i \"{inputPath}\" -o \"{outputPath}\"";
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

    public Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(string inputPath, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}