using Mutagen.Bethesda.Plugins.Records;
using NuGet.Packaging.Core;
using Spriggit.Core;

namespace Spriggit.Engine;

public record EngineEntryPoint(
    IEntryPoint<IMod, IModGetter> EntryPoint,
    PackageIdentity Package);