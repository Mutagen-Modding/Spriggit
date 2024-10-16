using System.IO.Abstractions;
using Mutagen.Bethesda;

namespace Spriggit.CLI.Lib.Commands.Sort;

public class SortCommandRunner
{
    private readonly IFileSystem _fileSystem;
    private readonly SortSkyrim _sortSkyrim;
    private readonly SortFallout4 _sortFallout4;
    private readonly SortStarfield _sortStarfield;

    public SortCommandRunner(
        IFileSystem fileSystem,
        SortSkyrim sortSkyrim,
        SortFallout4 sortFallout4,
        SortStarfield sortStarfield)
    {
        _fileSystem = fileSystem;
        _sortSkyrim = sortSkyrim;
        _sortFallout4 = sortFallout4;
        _sortStarfield = sortStarfield;
    }
    
    public async Task Run(SortCommand cmd)
    {
        ISort sorter;
        switch (cmd.GameRelease.ToCategory())
        {
            case GameCategory.Oblivion:
                // Nothing to do
                return;
            case GameCategory.Skyrim:
                sorter = _sortSkyrim;
                break;
            case GameCategory.Fallout4:
                sorter = _sortFallout4;
                break;
            case GameCategory.Starfield:
                sorter = _sortStarfield;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Console.WriteLine("Checking if there is sorting necessary.");
        if (!sorter.HasWorkToDo(cmd.InputPath, cmd.GameRelease, cmd.DataFolder))
        {
            Console.WriteLine("No sorting to be done.  Exiting.");
            return;
        }
        
        await sorter.Run(cmd.InputPath, cmd.GameRelease, cmd.OutputPath, cmd.DataFolder);
        Console.WriteLine("Sorting complete.  Exiting.");
    }
}