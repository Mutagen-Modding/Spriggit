using System.IO.Abstractions;
using CommandLine;
using Mutagen.Bethesda;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.CLI.Lib.Commands.Sort;

[Verb("sort-randomized-fields", HelpText = "Command to sort fields that often get randomized during mod editing")]
public class SortCommand
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

    [Option('k', "KnownMasterAnchorDirectory",
        HelpText = "Path to begin search for .spriggit file in order to locate Known Masters.  (Only required for separated master games, like Starfield)",
        Required = false)]
    public string? KnownMasterLocation { get; set; }

    private static SortRunnerContainer GetContainer()
    {
        return new SortRunnerContainer(new FileSystem(), LoggerSetup.Logger);
    }
    
    public async Task<int> Run()
    {
        await GetContainer()
            .Resolve().Value
            .Run(this);
        return 0;
    }
}