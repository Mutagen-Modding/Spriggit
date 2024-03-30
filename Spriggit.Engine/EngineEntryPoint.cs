using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using NuGet.Packaging.Core;
using Spriggit.Core;

namespace Spriggit.Engine;

public interface IEngineEntryPoint : IEntryPoint
{
    PackageIdentity Package { get; }
}