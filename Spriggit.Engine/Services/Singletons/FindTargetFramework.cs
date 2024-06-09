using System.IO.Abstractions;
using Noggog;
using Serilog;

namespace Spriggit.Engine.Services.Singletons;

public class FindTargetFramework
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly GetFrameworkType _getFrameworkType;

    public FindTargetFramework(
        ILogger logger,
        IFileSystem fileSystem, 
        GetFrameworkType getFrameworkType)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _getFrameworkType = getFrameworkType;
    }
    
    public string FindTargetFrameworkWithin(DirectoryPath packageDir)
    {
        var libDir = Path.Combine(packageDir, "lib");
        
        _logger.Information("Finding target framework for {Dir}", libDir);

        var targetFrameworkDir = _fileSystem.Directory
            .EnumerateDirectories(libDir)
            .Select(x => x)
            .OrderByDescending(x => x, new FrameworkLocatorComparer(_getFrameworkType, null))
            .FirstOrDefault();

        if (targetFrameworkDir == null)
        {
            throw new DirectoryNotFoundException($"Could not find target framework directory within {libDir}");
        }

        var targetFramework = Path.GetFileName(targetFrameworkDir);

        if (targetFramework == null)
        {
            throw new DirectoryNotFoundException($"Could not find target framework directory within {libDir}");
        }

        return targetFramework;
    }
}