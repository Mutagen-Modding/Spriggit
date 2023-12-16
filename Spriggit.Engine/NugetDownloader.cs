using Mutagen.Bethesda;
using Noggog;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.PackageExtraction;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using Spriggit.Core;
using ILogger = Serilog.ILogger;

namespace Spriggit.Engine;

public class NugetDownloader
{
    private readonly ILogger _logger;
    private readonly SourceCacheContext _cache = new();
    private readonly ISettings _settings;
    private readonly SourceRepositoryProvider _provider;
    
    public NugetDownloader(ILogger logger)
    {
        _logger = logger;
        _settings = Settings.LoadDefaultSettings(root: null);
        _provider = new SourceRepositoryProvider(_settings, Repository.Provider.GetCoreV3());
    }

    private static NuGetVersion? GetLatestVersion(IEnumerable<NuGetVersion> versions)
    {
        var ret = versions
            .Where(x => !x.IsPrerelease)
            .MaxBy(x => x.Version);
        if (ret != null) return ret;

        return versions.FirstOrDefault();
    }

    public async Task<PackageIdentity?> GetFirstIdentityFor(SpriggitMeta meta, CancellationToken cancellationToken)
    {
        if (!meta.Source.PackageName.EndsWith($".{meta.Release.ToCategory()}"))
        {
            var releaseSpecific = await GetFirstIdentityFor($"{meta.Source.PackageName}.{meta.Release.ToCategory()}", meta.Source.Version, cancellationToken);
            if (releaseSpecific != null)
            {
                return releaseSpecific;
            }
        }

        return await GetFirstIdentityFor(meta.Source.PackageName, meta.Source.Version, cancellationToken);
    }

    public async Task<PackageIdentity?> GetFirstIdentityFor(string packageName, string packageVersion, CancellationToken cancellationToken)
    {
        if (packageVersion.IsNullOrWhitespace())
        {
            _logger.Information("No version specified.  Checking NuGet repositories for latest version");
            var repos = _provider.GetRepositories().ToArray();

            if (repos.Length == 0)
            {
                _logger.Warning($"There were no nuget repositories listed!");
                return null;
            }
            
            foreach (var repository in repos)
            {
                _logger.Information($"  Looking in repo {repository}");
                FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();
                var versions = await resource.GetAllVersionsAsync(
                    packageName,
                    _cache,
                    NullLogger.Instance,
                    cancellationToken);
                var version = GetLatestVersion(versions);
                if (version == null) continue;

                return new PackageIdentity(packageName, version);
            }
        }
        else
        {
            return new PackageIdentity(packageName, NuGetVersion.Parse(packageVersion));
        }

        return null;
    }

    public async Task RestoreFor(PackageIdentity identity, DirectoryPath packagesPath, CancellationToken cancel)
    {
        FolderNuGetProject project = new FolderNuGetProject(packagesPath);

        NuGetPackageManager packageManager = new NuGetPackageManager(_provider, _settings, packagesPath)
        {
            PackagesFolderNuGetProject = project
        };
        bool allowPrereleaseVersions = true;
        bool allowUnlisted = false;
        ResolutionContext resolutionContext = new ResolutionContext(
            DependencyBehavior.Lowest, allowPrereleaseVersions, allowUnlisted, VersionConstraints.None);

        var clientPolicyContext = ClientPolicyContext.GetClientPolicy(_settings, NullLogger.Instance);
        INuGetProjectContext projectContext = new ConsoleProjectContext(NullLogger.Instance)
        {
            PackageExtractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv3,
                PackageExtractionBehavior.XmlDocFileSaveMode,
                clientPolicyContext,
                NullLogger.Instance
            )
        };
        
        var repos = _provider.GetRepositories().ToArray();
        await packageManager.InstallPackageAsync(packageManager.PackagesFolderNuGetProject,
            identity, resolutionContext, projectContext, repos,
            Array.Empty<SourceRepository>(),  // This is a list of secondary source repositories, probably empty
            cancel);
    }
    
    public async Task<PackageIdentity?> GetLatestIdentity(
        string packageName,
        string? packageVersion,
        CancellationToken cancellationToken)
    {
        if (packageVersion.IsNullOrWhitespace())
        {
            var repos = _provider.GetRepositories().ToArray();
            foreach (var repository in repos)
            {
                FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();
                var versions = await resource.GetAllVersionsAsync(
                    packageName,
                    _cache,
                    NullLogger.Instance,
                    cancellationToken);
                var version = GetLatestVersion(versions);
                if (version == null) continue;

                return new PackageIdentity(packageName, version);
            }
        }
        else
        {
            return new PackageIdentity(packageName, NuGetVersion.Parse(packageVersion));
        }

        return null;
    }
}