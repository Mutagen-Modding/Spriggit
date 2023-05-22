using CommandLine;
using Mutagen.Bethesda;
using Noggog;

namespace Spriggit.CLI.Commands;

[Verb("serialize", HelpText = "Converts a binary Bethesda plugin to a text based representation")]
public class SerializeCommand
{
    [Option('i', "InputPath", 
        HelpText = "Path to the Bethesda plugin",
        Required = true)]
    public FilePath InputPath { get; set; } = string.Empty;
    
    [Option('o', "OutputPath",
        HelpText = "Folder to export mod as its text representation",
        Required = true)]
    public DirectoryPath OutputPath { get; set; } = string.Empty;
    
    [Option('g', "GameRelease",
        HelpText = "Game release that the plugin is related to",
        Required = true)]
    public GameRelease GameRelease { get; set; }

    [Option('p', "PackageName",
        HelpText = "Spriggit serialization nuget package to use for conversion",
        Required = true)]
    public string PackageName { get; set; } = string.Empty;

    [Option('v', "PackageVersion",
        HelpText = "Spriggit serialization nuget package version to use for conversion",
        Required = false)]
    public string PackageVersion { get; set; } = string.Empty;
}