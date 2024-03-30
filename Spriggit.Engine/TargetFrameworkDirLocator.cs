using System.IO.Abstractions;
using Noggog;
using Serilog;

namespace Spriggit.Engine;

public class FrameworkLocatorComparer : IComparer<DirectoryPath>
{
    private readonly GetFrameworkType _getFrameworkType;
    private readonly string? _targetFramework;

    public FrameworkLocatorComparer(GetFrameworkType getFrameworkType, string? targetFramework)
    {
        _getFrameworkType = getFrameworkType;
        _targetFramework = targetFramework;
    }

    public int Compare(DirectoryPath x, DirectoryPath y)
    {
        var xName = Path.GetFileName(x.Path.AsSpan());
        var yName = Path.GetFileName(y.Path.AsSpan());
        if (!xName.StartsWith("net")) return -1;
        if (!yName.StartsWith("net")) return 1;
        var xTrim = xName.Slice(3);
        var yTrim = yName.Slice(3);

        var xType = _getFrameworkType.Get(xTrim, out var xNumber, out var xWin);
        var yType = _getFrameworkType.Get(yTrim, out var yNumber, out var yWin);

        var typeCompare = xType.CompareTo(yType);
        if (typeCompare != 0) return typeCompare;

        if (xWin != yWin)
        {
            if (xWin) return -1;
            return 1;
        }

        if (_targetFramework != null)
        {
            if (xName.SequenceEqual(_targetFramework))
            {
                return 1;
            }

            if (yName.SequenceEqual(_targetFramework))
            {
                return -1;
            }
        }

        return xNumber.CompareTo(yNumber);
    }
}

public class TargetFrameworkDirLocator
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly GetFrameworkType _getFrameworkType;

    public TargetFrameworkDirLocator(
        IFileSystem fileSystem,
        ILogger logger,
        GetFrameworkType getFrameworkType)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _getFrameworkType = getFrameworkType;
    }
    
    public DirectoryPath? GetTargetFrameworkDir(DirectoryPath packageDir, string targetFramework)
    {
        _logger.Information("Getting target framework directory for {Dir}", packageDir);
        var libDir = Path.Combine(packageDir.Path, "lib");
        if (!_fileSystem.Directory.Exists(libDir))
        {
            _logger.Information("Could not locate framework directory");
            return null;
        }

        var dirs = _fileSystem.Directory
            .EnumerateDirectories(libDir)
            .ToArray();

        var firstFrameworkDir = _fileSystem.Directory
            .EnumerateDirectories(libDir)
            .OrderByDescending(x => x, new FrameworkLocatorComparer(_getFrameworkType, targetFramework))
            .FirstOrDefault();

        if (firstFrameworkDir == null)
        {
            _logger.Information("Could not locate framework directory");
            return null;
        }

        _logger.Information("Framework directory located: {Path}", firstFrameworkDir);
        return firstFrameworkDir;
    }
}