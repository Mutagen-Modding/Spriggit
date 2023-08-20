using System.IO.Abstractions;
using Noggog;
using Serilog;

namespace Spriggit.Engine;

public class TargetFrameworkDirLocator : IComparer<DirectoryPath>
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public TargetFrameworkDirLocator(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }
    
    public DirectoryPath? GetTargetFrameworkDir(DirectoryPath packageDir)
    {
        _logger.Information("Getting target framework directory for {Dir}", packageDir);
        var libDir = Path.Combine(packageDir.Path, "lib");
        if (!_fileSystem.Directory.Exists(libDir))
        {
            _logger.Information("Could not locate framework directory");
            return null;
        }
        
        var firstFrameworkDir = _fileSystem.Directory
            .EnumerateDirectories(libDir)
            .OrderByDescending(x => x, this)
            .FirstOrDefault();

        if (firstFrameworkDir == null)
        {
            _logger.Information("Could not locate framework directory");
            return null;
        }

        _logger.Information("Framework directory located: {Path}", firstFrameworkDir);
        return firstFrameworkDir;
    }

    private enum FrameworkType
    {
        Framework,
        CoreApp,
        Core,
        Standard,
        Net
    }

    private FrameworkType GetFrameworkType(
        ReadOnlySpan<char> span, 
        out int number,
        out bool windows)
    {
        var orig = span;
        if (span.StartsWith("standard"))
        {
            number = default;
            windows = default;
            return FrameworkType.Standard;
        }
        
        if (span.StartsWith("coreapp"))
        {
            number = default;
            windows = default;
            return FrameworkType.CoreApp;
        }
        
        if (span.StartsWith("core"))
        {
            number = default;
            windows = default;
            return FrameworkType.Core;
        }

        var winIndex = span.IndexOf("-windows");
        if (winIndex != -1)
        {
            span = span.Slice(0, winIndex);
        }

        windows = winIndex != -1;
        
        if (!char.IsNumber(span[0])
            || !double.TryParse(span, out var d))
        {
            throw new ArgumentException(nameof(span), $"Could not parse framework type for {orig.ToString()}");
        }

        number = (int)d;
        if (number > 10)
        {
            return FrameworkType.Framework;
        }

        return FrameworkType.Net;
    }

    public int Compare(DirectoryPath x, DirectoryPath y)
    {
        var xName = Path.GetFileName(x.Path.AsSpan());
        var yName = Path.GetFileName(y.Path.AsSpan());
        if (!xName.StartsWith("net")) return -1;
        if (!yName.StartsWith("net")) return 1;
        var xTrim = xName.Slice(3);
        var yTrim = yName.Slice(3);

        var xType = GetFrameworkType(xTrim, out var xNumber, out var xWin);
        var yType = GetFrameworkType(yTrim, out var yNumber, out var yWin);

        var typeCompare = xType.CompareTo(yType);
        if (typeCompare != 0) return typeCompare;

        if (xWin != yWin)
        {
            if (xWin) return -1;
            return 1;
        }

        return xNumber.CompareTo(yNumber);
    }
}