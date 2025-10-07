using CommandLine;
using Mutagen.Bethesda;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.Standardize;

[Verb("standardize", HelpText = "Testing command to attempt to standardize a mod for binary comparison")]
public class StandardizeCommand
{
    [Option('i', "InputPath", 
        HelpText = "Path to the Bethesda plugin",
        Required = true)]
    public FilePath InputPath { get; set; } = string.Empty;
    
    [Option('o', "OutputPath", 
        HelpText = "Path to output the standardized Bethesda plugin",
        Required = true)]
    public FilePath OutputPath { get; set; } = string.Empty;
    
    [Option('g', "GameRelease",
        HelpText = "Game release that the plugin is related to",
        Required = true)]
    public GameRelease GameRelease { get; set; }
}