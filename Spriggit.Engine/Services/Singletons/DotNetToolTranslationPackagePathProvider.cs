using Noggog;
using NuGet.Packaging.Core;

namespace Spriggit.Engine.Services.Singletons;

public class DotNetToolTranslationPackagePathProvider
{
    public DirectoryPath Path(DirectoryPath tempPath, PackageIdentity ident) => new(
        System.IO.Path.Combine(tempPath, "Translations", ident.Id, ident.Version.ToString()));
}