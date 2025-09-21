using CommandLine;

namespace Spriggit.CLI.Lib.Commands.FormIDCollision;

[Verb("formid-collision", HelpText = "Command to be run after a git merge to address FormID collisions")]
public class FormIDCollisionCommand
{
    [Option('p', "SpriggitPath", HelpText = "Path to the Bethesda plugin as its text representation", Required = true)]
    public string SpriggitPath { get; set; } = string.Empty;

    [Option("Debug",
        HelpText = "Set up for debug mode, including resetting nuget caches",
        Required = false)]
    public bool Debug { get; set; }

    [Option('d', "DataFolder",
        HelpText = "Path to the data folder to look to for mod files.  (Only required for separated master games, like Starfield)",
        Required = false)]
    public string? DataFolder { get; set; }
}