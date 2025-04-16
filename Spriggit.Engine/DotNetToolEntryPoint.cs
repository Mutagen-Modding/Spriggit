using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.Processes.DI;
using Noggog.WorkEngine;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Serilog;
using Spriggit.Core;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.Engine;

public class DotNetToolEntryPoint : IEngineEntryPoint
{
    private readonly ILogger _logger;
    private readonly ProcessFactory _processFactory;
    private readonly PackageIdentity _package;
    private readonly DotNetToolTranslationPackagePathProvider _pathProvider;
    private readonly SpriggitTempSourcesProvider _tempSourcesProvider;
    private readonly NuGetVersion _unknownThrowVersion = new(0, 36, 15, 0);
    
    public DotNetToolEntryPoint(
        PackageIdentity package,
        ILogger logger,
        ProcessFactory processFactory,
        DotNetToolTranslationPackagePathProvider pathProvider,
        SpriggitTempSourcesProvider tempSourcesProvider)
    {
        _logger = logger;
        _processFactory = processFactory;
        _package = package;
        _pathProvider = pathProvider;
        _tempSourcesProvider = tempSourcesProvider;
    }

    private string GetDataPathParam(string? dataPath)
    {
        if (dataPath != null)
        {
            return $" -d \"{dataPath}\"";
        }

        return string.Empty;
    }

    private string GetThrowOnUnknownParam(bool throwOnUnknown)
    {
        if (!throwOnUnknown && _package.Version >= _unknownThrowVersion)
        {
            return $" -u \"False\"";
        }

        return string.Empty;
    }
    
    private FilePath GetExePath()
    {
        DirectoryPath toolsPath = _pathProvider.Path(
            _tempSourcesProvider.SpriggitTempPath,
            _package);
        return Path.Combine(toolsPath, $"{_package.Id}{(System.OperatingSystem.IsLinux() ? string.Empty : ".exe")}");
    }
    
    public async Task Serialize(ModPath modPath, DirectoryPath outputDir, DirectoryPath? dataPath, KnownMaster[] knownMasters,
        GameRelease release, IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator,
        SpriggitSource meta, bool throwIfUnknown, CancellationToken cancel)
    {
        var exePath = GetExePath();

        var args = $"serialize -i \"{modPath.Path.Path}\" -o \"{outputDir.Path}\" -g {release} -p {_package.Version.ToString().TrimStringFromEnd(".0")}{GetDataPathParam(dataPath)}{GetThrowOnUnknownParam(throwIfUnknown)}";
        _logger.Information("Running DotNet Entry point serialize with Args: {Args}", args);
        using var processWrapper = _processFactory.Create(
            new ProcessStartInfo(exePath)
            {
                Arguments = args
            },
            killWithParent: !RuntimeInformation.IsOSPlatform(OSPlatform.Linux));
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
        var ret = await processWrapper.Run();
        if (ret != 0)
        {
            throw new InvalidOperationException("Failed to serialize");
        }
    }

    public async Task Deserialize(string inputPath, string outputPath, DirectoryPath? dataPath, KnownMaster[] knownMasters,
        IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator, CancellationToken cancel)
    {
        var exePath = GetExePath();

        var args = $"deserialize -i \"{inputPath}\" -o \"{outputPath}\" -p {_package.Id} -v {_package.Version.ToString().TrimStringFromEnd(".0")}{GetDataPathParam(dataPath)}";
        _logger.Information("Running DotNet Tool entry point deserialize with Args: {Args}", args);
        using var processWrapper = _processFactory.Create(
            new ProcessStartInfo(exePath)
            {
                Arguments = args
            },
            killWithParent: !RuntimeInformation.IsOSPlatform(OSPlatform.Linux));
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
        var ret = await processWrapper.Run();
        if (ret != 0)
        {
            throw new InvalidOperationException("Failed to deserialize");
        }
    }

    public void Dispose()
    {
    }

    public PackageIdentity Package => _package;
}