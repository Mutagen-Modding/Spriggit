using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.Sort;

public interface ISort
{
    bool HasWorkToDo(
        ModPath path,
        GameRelease release,
        KeyedMasterStyle[] knownMasters,
        DirectoryPath? dataFolder);
    Task Run(
        ModPath path,
        GameRelease release,
        ModPath outputPath,
        KeyedMasterStyle[] knownMasters,
        DirectoryPath? dataFolder);
}