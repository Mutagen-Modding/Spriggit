using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Records;
using Serilog;
using Spriggit.CLI.Lib.Commands.Sort.Services.Fallout4;
using Spriggit.CLI.Lib.Commands.Sort.Services.Skyrim;
using Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;
using Spriggit.Core;
using Spriggit.Core.Services.Singletons;

namespace Spriggit.CLI.Lib.Commands.Sort.Services;

public class SortCommandRunner
{
    private readonly ILogger _logger;
    private readonly SpriggitFileLocator _spriggitFileLocator;
    private readonly SortSkyrim _sortSkyrim;
    private readonly SortFallout4 _sortFallout4;
    private readonly SortStarfield _sortStarfield;

    public SortCommandRunner(
        ILogger logger,
        SpriggitFileLocator spriggitFileLocator,
        SortSkyrim sortSkyrim,
        SortFallout4 sortFallout4,
        SortStarfield sortStarfield)
    {
        _logger = logger;
        _spriggitFileLocator = spriggitFileLocator;
        _sortSkyrim = sortSkyrim;
        _sortFallout4 = sortFallout4;
        _sortStarfield = sortStarfield;
    }

    private SpriggitFile? GetSpriggitFile(SortCommand cmd)
    {
        if (cmd.KnownMasterLocation != null)
        {
            _logger.Information("Using spriggit file from: {Path}", cmd.KnownMasterLocation);
            return _spriggitFileLocator.LocateAndParse(cmd.KnownMasterLocation);
        }

        var ret = _spriggitFileLocator.LocateAndParse(Path.GetDirectoryName(cmd.OutputPath)!);
        if (ret != null)
        {
            _logger.Information("Using spriggit file from: {Path}", cmd.OutputPath);
            return ret;
        }

        ret = _spriggitFileLocator.LocateAndParse(Path.GetDirectoryName(cmd.InputPath)!);
        if (ret != null)
        {
            _logger.Information("Using spriggit file from: {Path}", cmd.InputPath);
            return ret;
        }

        return null;
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

        KeyedMasterStyle[] keyedMasterStyles = GetSpriggitFile(cmd)?.KnownMasters
            .Select(x => new KeyedMasterStyle(x.ModKey, x.Style))
            .ToArray() ?? [];

        Console.WriteLine("Checking if there is sorting necessary.");
        if (!sorter.HasWorkToDo(cmd.InputPath, cmd.GameRelease, keyedMasterStyles, cmd.DataFolder))
        {
            Console.WriteLine("No sorting to be done.  Exiting.");
            return;
        }
        
        await sorter.Run(cmd.InputPath, cmd.GameRelease, cmd.OutputPath, keyedMasterStyles, cmd.DataFolder);
        Console.WriteLine("Sorting complete.  Exiting.");
    }
}