using System.IO.Abstractions;
using Noggog;

namespace Spriggit.Engine.Services.Singletons;

public class GitFolderLocator
{
    private readonly IFileSystem _fileSystem;

    public GitFolderLocator(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public DirectoryPath? Get(DirectoryPath? startingPath)
    {
        while (true)
        {
            if (startingPath == null) return null;
            var gitPath = Path.Combine(startingPath, ".git");
            if (_fileSystem.Directory.Exists(gitPath))
            {
                return gitPath;
            }

            startingPath = Path.GetDirectoryName(startingPath);
        }
    }
}