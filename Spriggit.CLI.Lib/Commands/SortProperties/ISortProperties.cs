using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.SortProperties;

public interface ISortProperties
{
    bool HasWorkToDo(
        ModPath path,
        GameRelease release,
        DirectoryPath? dataFolder);
    Task Run(
        ModPath path,
        GameRelease release,
        ModPath outputPath,
        DirectoryPath? dataFolder);
}