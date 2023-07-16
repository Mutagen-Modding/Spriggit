using NuGet.Packaging.Core;
using Spriggit.Core;

namespace Spriggit.Engine;

public record EngineEntryPoint(
    IEntryPoint EntryPoint,
    PackageIdentity Package);