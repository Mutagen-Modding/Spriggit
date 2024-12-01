using System.Diagnostics;
using Noggog;
using Noggog.Processes.DI;
using NuGet.Packaging.Core;
using Serilog;

namespace Spriggit.Engine.Services.Singletons;

public class ConstructDotNetToolEndpoint
{
    private readonly ILogger _logger;
    private readonly ProcessFactory _processFactory;
    private readonly DotNetToolTranslationPackagePathProvider _pathProvider;
    private readonly SpriggitTempSourcesProvider _tempSourcesProvider;

    public ConstructDotNetToolEndpoint(
        ILogger logger,
        ProcessFactory processFactory,
        DotNetToolTranslationPackagePathProvider pathProvider,
        SpriggitTempSourcesProvider tempSourcesProvider)
    {
        _logger = logger;
        _processFactory = processFactory;
        _pathProvider = pathProvider;
        _tempSourcesProvider = tempSourcesProvider;
    }

    public async Task<DotNetToolEntryPoint?> ConstructFor(
        DirectoryPath tempPath,
        PackageIdentity ident,
        CancellationToken cancellationToken)
    {
        DirectoryPath toolsPath = _pathProvider.Path(tempPath, ident);

        if (!toolsPath.CheckExists())
        {
            try
            {
                var args = $"tool install {ident.Id} --version {ident.Version} --tool-path \"{toolsPath}\"";
                _logger.Information("Running DotNet Entry point serialize with Args: {Args}", args);
                using var processWrapper = _processFactory.Create(
                    new ProcessStartInfo("dotnet")
                    {
                        Arguments = args,
                    },
                    cancel: cancellationToken);
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
            Log.ForContext<DotNetToolEntryPoint>(),
            _processFactory,
            _pathProvider,
            _tempSourcesProvider);
    }
}