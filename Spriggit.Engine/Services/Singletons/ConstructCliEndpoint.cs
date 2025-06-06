﻿using Noggog;
using Noggog.Processes.DI;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Serilog;

namespace Spriggit.Engine.Services.Singletons;

public class ConstructCliEndpoint
{
    private readonly ILogger _logger;
    private readonly ProcessFactory _processFactory;
    private readonly PrepareCliFolder _prepareCliFolder;
    private readonly PackageVersioningChecker _packageVersioningChecker;

    public ConstructCliEndpoint(
        ILogger logger,
        ProcessFactory processFactory,
        PrepareCliFolder prepareCliFolder, 
        PackageVersioningChecker packageVersioningChecker)
    {
        _logger = logger;
        _processFactory = processFactory;
        _prepareCliFolder = prepareCliFolder;
        _packageVersioningChecker = packageVersioningChecker;
    }

    private bool IsSameCliVersion(NuGetVersion version)
    {
        var assemblyVersion = AssemblyVersions.For<ConstructCliEndpoint>().ProductVersion;
        if (assemblyVersion == null) return false;
        var indexOfPlus = assemblyVersion.IndexOf("+", StringComparison.InvariantCulture);
        if (indexOfPlus != -1)
        {
            assemblyVersion = assemblyVersion.Substring(0, indexOfPlus);
        }

        return assemblyVersion.TrimStringFromEnd(".0").Equals(version.ToString().TrimStringFromEnd(".0"));
    }

    public async Task<CliEntryPoint?> ConstructFor(
        DirectoryPath tempPath,
        PackageIdentity ident,
        CancellationToken cancellationToken)
    {
        // ToDo
        // Don't assume it's an official spriggit translation package.
        // If it wasn't we would need to fish for spriggit dependencies
        var cliVersion = ident.Version;
        
        if (IsSameCliVersion(cliVersion))
        {
            _logger.Information("Not making CLI endpoint, as it was the same version: {Version}", cliVersion);
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
            _packageVersioningChecker,
            ident);
    }
}