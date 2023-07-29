using System.IO.Abstractions;
using Noggog;
using Spriggit.Core;

namespace Spriggit.Engine;

public class GetDefaultEntryPoint
{
    private readonly IFileSystem _fileSystem;
    private readonly EntryPointCache _entryPointCache;
    private readonly NugetDownloader _nugetDownloader;
    private const string RecordDataFileNameWithoutExtension = "RecordData";

    public GetDefaultEntryPoint(IFileSystem fileSystem,
        EntryPointCache entryPointCache,
        NugetDownloader nugetDownloader)
    {
        _fileSystem = fileSystem;
        _entryPointCache = entryPointCache;
        _nugetDownloader = nugetDownloader;
    }
    
    private static string PackageSuffix(FileName fileName)
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
                if (Path.GetFileNameWithoutExtension(f).Equals(RecordDataFileNameWithoutExtension,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return PackageSuffix(Path.GetFileName(f));
                }
            }
        }
        throw new FileNotFoundException($"Could not find expected meta file at {spriggitPluginPath}");
    }
    
    public async Task<IEntryPoint> Get(string spriggitPluginPath, CancellationToken cancel)
    {
        var suffix = GetPackageStyleSuffix(spriggitPluginPath);
        var packageName = $"Spriggit.{suffix}.Skyrim";
        var ident = await _nugetDownloader.GetFirstIdentityFor(
            packageName: packageName, packageVersion: string.Empty, cancellationToken: cancel);
        
        var entryPoint = await _entryPointCache.GetFor(ident, cancel);
        return entryPoint != null ? entryPoint.EntryPoint 
            : throw new NotSupportedException($"Could not get default entry point for {spriggitPluginPath}");
    }
}