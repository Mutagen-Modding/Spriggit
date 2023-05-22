using System.IO.Abstractions;
using Mutagen.Bethesda.Serialization.Utility;
using Noggog;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Spriggit.Core;

namespace Spriggit.Engine;

public class GetDefaultEntryPoint
{
    private readonly IFileSystem _fileSystem;
    private readonly ConstructEntryPoint _constructEntryPoint;

    public GetDefaultEntryPoint(IFileSystem fileSystem,
        ConstructEntryPoint constructEntryPoint)
    {
        _fileSystem = fileSystem;
        _constructEntryPoint = constructEntryPoint;
    }
    
    private string PackageSuffix(FileName fileName)
    {
        if (fileName.Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase))
        {
            return "Yaml";
        }
        if (fileName.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            return "Json";
        }

        throw new NotImplementedException($"No default entry point for suffix type: {fileName.Extension}");
    }

    private string GetPackageStyleSuffix(string spriggitPluginPath)
    {
        if (_fileSystem.File.Exists(spriggitPluginPath))
        {
            return PackageSuffix(Path.GetFileName(spriggitPluginPath));
        }
        
        if (_fileSystem.Directory.Exists(spriggitPluginPath))
        {
            foreach (var f in _fileSystem.Directory.GetFiles(spriggitPluginPath))
            {
                if (Path.GetFileNameWithoutExtension(f).Equals(SerializationHelper.RecordDataFileNameWithoutExtension,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return PackageSuffix(Path.GetFileName(f));
                }
            }
        }
        throw new FileNotFoundException($"Could not find expected meta file at {spriggitPluginPath}");
    }
    
    public async Task<IEntryPoint> Get(string spriggitPluginPath)
    {
        var suffix = GetPackageStyleSuffix(spriggitPluginPath);
        var packageName = $"Spriggit.{suffix}.Skyrim";

        var ret = await _constructEntryPoint.ConstructFor(packageName, packageVersion: null,
            cancellationToken: CancellationToken.None);
        if (ret == null)
        {
            throw new NotSupportedException($"Could not get default entry point for {spriggitPluginPath}");
        }

        return ret;
    }
}