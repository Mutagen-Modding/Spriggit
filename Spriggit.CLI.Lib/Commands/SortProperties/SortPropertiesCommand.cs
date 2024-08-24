using System.IO.Abstractions;
using CommandLine;
using Mutagen.Bethesda;

namespace Spriggit.CLI.Lib.Commands.SortProperties;

[Verb("sort-script-properties", HelpText = "Command to sort script properties, which often get randomized")]
public class SortPropertiesCommand
{
    [Option('i', "InputPath", HelpText = "Path to the Bethesda plugin to process", Required = true)]
    public string InputPath { get; set; } = string.Empty;
    
    [Option('o', "OutputPath", HelpText = "Path to the Bethesda plugin to process", Required = true)]
    public string OutputPath { get; set; } = string.Empty;
    
    [Option('g', "GameRelease",
        HelpText = "Game release that the plugin is related to",
        Required = true)]
    public GameRelease GameRelease { get; set; }

    [Option('d', "DataFolder",
        HelpText = "Path to the data folder to look to for mod files.  (Only required for separated master games, like Starfield)",
        Required = false)]
    public string? DataFolder { get; set; }

    public async Task<int> Run()
    {
        ISortProperties sorter;
        switch (GameRelease.ToCategory())
        {
            case GameCategory.Oblivion:
                // Nothing to do
                return 0;
            case GameCategory.Skyrim:
                sorter = new SortPropertiesSkyrim(new FileSystem());
                break;
            case GameCategory.Fallout4:
                sorter = new SortPropertiesFallout4();
                break;
            case GameCategory.Starfield:
                sorter = new SortPropertiesStarfield();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await sorter.Run(InputPath, GameRelease, OutputPath, DataFolder);
        return 0;
    }
}