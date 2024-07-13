using Noggog;
using Noggog.Processes.DI;
using NuGet.Packaging.Core;
using Serilog;
using Serilog.Core;

namespace Spriggit.Engine.Services.Singletons;

public class ConstructCliEndpoint
{
    private readonly ILogger _logger;
    private readonly ProcessFactory _processFactory;
    private readonly PrepareCliFolder _prepareCliFolder;

    public ConstructCliEndpoint(
        ILogger logger,
        ProcessFactory processFactory,
        PrepareCliFolder prepareCliFolder)
    {
        _logger = logger;
        _processFactory = processFactory;
        _prepareCliFolder = prepareCliFolder;
    }

    public async Task<CliEntryPoint?> ConstructFor(
        DirectoryPath tempPath,
        PackageIdentity ident,
        CancellationToken cancellationToken)
    {
        // ToDo
        // Don't assume it's a spriggit translation package
        // Would need to fish for spriggit dependencies in that case
        var cliVersion = ident.Version;
        
        if (AssemblyVersions.For<ConstructCliEndpoint>().ProductVersion?.Equals(cliVersion.ToString()) ?? false)
        {
            return null;
        }
            
        var clisPath = Path.Combine(tempPath, SpriggitTempSourcesProvider.ClisSubPath);
        var cliUnpackFolder = new DirectoryPath(Path.Combine(clisPath, cliVersion.ToString()));
        
        if (!cliUnpackFolder.CheckExists())
        {
            try
            {
                await _prepareCliFolder.Prepare(cliVersion, cancellationToken, cliUnpackFolder);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error preparing clis folder");
                return null;
            }
        }

        var exePath = Path.Combine(cliUnpackFolder, "Spriggit.CLI.exe");
        return new CliEntryPoint(
            Log.ForContext<CliEntryPoint>(),
            _processFactory,
            exePath,
            ident);
    }
}