using System.IO.Abstractions;
using Mutagen.Bethesda.Serialization.Utility;
using Noggog;
using Spriggit.Core;

namespace Spriggit.Engine;

public class GetDefaultEntryPoint
{
    private readonly IFileSystem _fileSystem;
    private readonly EntryPointCache _entryPointCache;
    private readonly NugetDownloader _nugetDownloader;

    public GetDefaultEntryPoint(IFileSystem fileSystem,
        EntryPointCache entryPointCache,
        NugetDownloader nugetDownloader)
    {
        _fileSystem = fileSystem;
        _entryPointCache = entryPointCache;
        _nugetDownloader = nugetDownloader;
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
        var ident = await _nugetDownloader.GetFirstIdentityFor(packageName, string.Empty, CancellationToken.None);
        
        var ret = await _entryPointCache.GetFor(ident);
        if (ret == null)
        {
            throw new NotSupportedException($"Could not get default entry point for {spriggitPluginPath}");
        }

        return ret.EntryPoint;
    }
}