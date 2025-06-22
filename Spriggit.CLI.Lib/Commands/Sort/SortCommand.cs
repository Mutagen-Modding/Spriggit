using System.IO.Abstractions;
using Autofac;
using CommandLine;
using Mutagen.Bethesda;
using Serilog;
using Spriggit.CLI.Lib.Commands.Sort.Services;

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
        HelpText = "Path to the data folder to look to for mod files.  (Only required for separated master games, like Starfield)   ",
        Required = false)]
    public string? DataFolder { get; set; }

    [Option('k', "KnownMasterAnchorDirectory",
        HelpText = "Path to begin search for .spriggit file in order to locate Known Masters.  (Only required for separated master games, like Starfield)",
        Required = false)]
    public string? KnownMasterLocation { get; set; }

    internal static SortCommandRunner GetService()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<SortModule>();
        builder.RegisterInstance(new FileSystem())
            .As<IFileSystem>();
        builder.RegisterInstance(LoggerSetup.Logger)
            .As<ILogger>();
        var container = builder.Build();
        return container.Resolve<SortCommandRunner>();
    }
    
    public async Task<int> Run()
    {
        await GetService()
            .Run(this);
        return 0;
    }
}