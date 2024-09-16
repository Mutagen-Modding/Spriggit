using System.IO.Abstractions;
using Noggog;

namespace Spriggit.Engine.Services.Singletons;

public class SerializeBlocker
{
    private readonly IFileSystem _fileSystem;

    public SerializeBlocker(
        IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public void CheckAndBlock(DirectoryPath outputFolder)
    {
        var spriggitFile = Path.Combine(outputFolder, SpriggitFileLocator.ConfigFileName);
        if (_fileSystem.File.Exists(spriggitFile))
        {
            throw new InvalidOperationException($"Cannot export next to a {SpriggitFileLocator.ConfigFileName} file.  Target should be a dedicated folder.");
        }
        
        var gitFolder = Path.Combine(outputFolder, ".git");
        if (_fileSystem.Directory.Exists(gitFolder))
        {
            throw new InvalidOperationException("Cannot export next to a .git folder.  Target should be a dedicated folder.");
        }
    }
}