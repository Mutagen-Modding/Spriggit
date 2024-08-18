using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Meta;
using Noggog;

namespace Spriggit.Engine.Services.Singletons;

public class DataPathChecker
{
    private readonly IFileSystem _fileSystem;

    public DataPathChecker(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public void CheckDataPath(
        GameRelease release,
        DirectoryPath? dataPath)
    {
        var constants = GameConstants.Get(release);
        if (!constants.SeparateMasterLoadOrders) return;
        if (dataPath == null)
        {
            throw new ArgumentException($"Data folder is required for games with separated master load orders.{Environment.NewLine}Specify an empty string to override and use no data folder anyway.");
        }
        if (dataPath.Value.Path == string.Empty) return;

        if (!_fileSystem.Directory.Exists(dataPath))
        {
            throw new DirectoryNotFoundException("Data folder was not found");
        }
    }
}