using System.Diagnostics;
using System.IO.Abstractions;
using Noggog;
using Noggog.Processes.DI;
using NuGet.Packaging.Core;
using Serilog;

namespace Spriggit.Engine.Services.Singletons;

public class ConstructDotNetToolEndpoint
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly ProcessFactory _processFactory;
    private readonly DebugState _debugState;
    private readonly DotNetToolTranslationPackagePathProvider _pathProvider;
    private readonly SpriggitTempSourcesProvider _tempSourcesProvider;

    public ConstructDotNetToolEndpoint(
        ILogger logger,
        IFileSystem fileSystem,
        ProcessFactory processFactory,
        DebugState debugState,
        DotNetToolTranslationPackagePathProvider pathProvider,
        SpriggitTempSourcesProvider tempSourcesProvider)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _processFactory = processFactory;
        _debugState = debugState;
        _pathProvider = pathProvider;
        _tempSourcesProvider = tempSourcesProvider;
    }

    private async Task<bool> IsToolInstalled(
        PackageIdentity ident,
        DirectoryPath toolsPath,
        CancellationToken cancellationToken)
    {
        var args = $"tool list {ident.Id} --tool-path \"{toolsPath}\"";
        var ret = _processFactory.Create(
            new ProcessStartInfo("dotnet")
            {
                Arguments = args,
            },
            cancel: cancellationToken,
            killWithParent: false);
        bool found = false;
        using var outputSub = ret.Output
            .Subscribe(x =>
            {
                if (found) return;
                var split = x.Split("      ");
                if (split.Length != 3) return;
                if (split[0].Equals(ident.Id, StringComparison.OrdinalIgnoreCase) && split[1] == ident.Version.ToString())
                {
                    found = true;
                }
            });
        await ret.Run();
        return found;
    }

    public async Task<DotNetToolEntryPoint?> ConstructFor(
        DirectoryPath tempPath,
        PackageIdentity ident,
        CancellationToken cancellationToken)
    {
        DirectoryPath toolsPath = _pathProvider.Path(tempPath, ident);

        if (_debugState.ClearNugetSources)
        {
            _logger.Information("In debug mode.  Forcing dotnet tool reinstall");
        }
        
        if (_debugState.ClearNugetSources || !await IsToolInstalled(ident, toolsPath, cancellationToken))
        {
            try
            {
                _logger.Information("Running DotNet Entry point install.  Clearing old path {ToolsPath}", toolsPath);
                _fileSystem.Directory.DeleteEntireFolder(toolsPath);
                var args = $"tool install {ident.Id} --version {ident.Version} --tool-path \"{toolsPath}\"";
                _logger.Information("Running DotNet Entry point install with Args: {Args}", args);
                using var processWrapper = _processFactory.Create(
                    new ProcessStartInfo("dotnet")
                    {
                        Arguments = args,
                    },
                    cancel: cancellationToken,
                    killWithParent: false);
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
                    _logger.Error("Error running dotnet tool install for {Identity}", ident);
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error preparing dotnet tool folder for {Identity}", ident);
                return null;
            }
        }

        return new DotNetToolEntryPoint(
            ident,
            _logger.ForContext<DotNetToolEntryPoint>(),
            _processFactory,
            _pathProvider,
            _tempSourcesProvider);
    }
}