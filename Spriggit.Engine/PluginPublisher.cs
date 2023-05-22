using System.IO.Abstractions;
using Noggog;

namespace Spriggit.Engine;

public class PluginPublisher
{
    private readonly IFileSystem _fileSystem;
    private readonly TargetFrameworkDirLocator _frameworkDirLocator;

    public PluginPublisher(
        IFileSystem fileSystem,
        TargetFrameworkDirLocator frameworkDirLocator)
    {
        _fileSystem = fileSystem;
        _frameworkDirLocator = frameworkDirLocator;
    }
    
    public void Publish(DirectoryPath packageRepoDir, DirectoryPath outputDir)
    {
        // ToDo
        // Lean on dotnet SDK if it's installed
        
        // A basic naive publish attempt.  Just loops packages pulled in, and
        // copies in DLLs from the best approximated target framework
        foreach (var packageDir in _fileSystem.Directory.EnumerateDirectories(packageRepoDir.Path))
        {
            var frameworkDir = _frameworkDirLocator.GetTargetFrameworkDir(packageDir);
            if (frameworkDir == null) continue;
            if (outputDir == frameworkDir) continue;
            _fileSystem.Directory.DeepCopy(frameworkDir.Value, outputDir, overwrite: true);
        }
    }
}