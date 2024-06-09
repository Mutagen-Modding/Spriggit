using CommandLine;

namespace Spriggit.CLI.Commands;

[Verb("merge-version-syncer", HelpText = "Command to be run after a git merge to address differing spriggit versions between parents")]
public class MergeVersionSyncerCommand
{
    [Option('p', "SpriggitPath", HelpText = "Path to the Bethesda plugin as its text representation", Required = true)]
    public string SpriggitPath { get; set; } = string.Empty;

    [Option('d', "Debug",
        HelpText = "Set up for debug mode, including resetting nuget caches",
        Required = false)]
    public bool Debug { get; set; }
}