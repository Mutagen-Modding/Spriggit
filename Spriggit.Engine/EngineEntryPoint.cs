using NuGet.Packaging.Core;
using Spriggit.Core;

namespace Spriggit.Engine;

public interface IEngineEntryPoint : IEntryPoint, IDisposable
{
    PackageIdentity Package { get; }
}