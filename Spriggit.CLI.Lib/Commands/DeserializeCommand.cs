using CommandLine;
using Noggog;

namespace Spriggit.CLI.Commands;

[Verb("deserialize", aliases: new[] { "create-plugin", "convert-to-plugin" }, HelpText = "Converts a mod from its text based representation to a binary Bethesda plugin")]
public class DeserializeCommand
{
    [Option('i', "InputPath", HelpText = "Path to the Bethesda plugin folder as its Spriggit text representation", Required = true)]
    public string InputPath { get; set; } = string.Empty;
    
    [Option('o', "OutputPath", HelpText = "Path to export the mod as its Bethesda plugin representation", Required = true)]
    public FilePath OutputPath { get; set; } = string.Empty;

    [Option('p', "PackageName",
        HelpText = "Spriggit serialization nuget package to use for conversion.  Leave blank to auto detect",
        Required = false)]
    public string PackageName { get; set; } = string.Empty;

    [Option('v', "PackageVersion",
        HelpText = "Spriggit serialization nuget package version to use for conversion.  Leave blank to auto detect",
        Required = false)]
    public string PackageVersion { get; set; } = string.Empty;

    [Option('t', "Threads",
        HelpText = "Maximum number of threads to use",
        Required = false)]
    public byte? Threads { get; set; }

    [Option('d', "Debug",
        HelpText = "Set up for debug mode, including resetting nuget caches",
        Required = false)]
    public bool Debug { get; set; }
}