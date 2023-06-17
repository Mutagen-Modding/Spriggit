using Mutagen.Bethesda;
using Noggog;
using Spriggit.Engine;
using Spriggit.UI.Settings;

namespace Spriggit.UI.Services;

public class PackageInputQuery
{
    private readonly NugetDownloader _nugetDownloader;
    private readonly LinkSourceCategoryToPackageName _packageName;

    public PackageInputQuery(
        NugetDownloader nugetDownloader,
        LinkSourceCategoryToPackageName packageName)
    {
        _nugetDownloader = nugetDownloader;
        _packageName = packageName;
    }

    public async Task<GetResponse<string>> GetVersionToUse(
        LinkSourceCategory sourceCategory,
        GameRelease release,
        string packageName,
        string? inputVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!inputVersion.IsNullOrWhitespace()) return inputVersion;
            var nameToUse = _packageName.GetPackageName(sourceCategory, release, packageName);
            if (nameToUse.Failed) return nameToUse;
            packageName = nameToUse.Value;
            var latestIdent = await _nugetDownloader.GetLatestIdentity(packageName, inputVersion, cancellationToken);
            if (latestIdent == null) return GetResponse<string>.Fail("Could not find latest version of package");
            return latestIdent.Version.ToString();
        }
        catch (Exception e)
        {
            return GetResponse<string>.Fail(e);
        }
    }
}