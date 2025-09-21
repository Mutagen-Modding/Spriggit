using CommandLine;

namespace Spriggit.CLI.Lib.Commands.UpgradeTargetSpriggitVersionCommand;

[Verb("upgrade", HelpText = "Upgrades and reserializes files to a newer version of Spriggit")]
public class UpgradeTargetSpriggitVersionCommand
{
    [Option('p', "SpriggitPath", HelpText = "Path to the Bethesda plugin as its text representation", Required = true)]
    public string SpriggitPath { get; set; } = string.Empty;
    
    [Option('d', "DataFolder",
        HelpText = "Path to the data folder to look to for mod files.  (Only required for separated master games, like Starfield)",
        Required = false)]
    public string? DataFolder { get; set; }

    [Option('v', "PackageVersion",
        HelpText = "Spriggit serialization nuget package version to change to.  Null will upgrade to latest.",
        Required = false)]
    public string? PackageVersion { get; set; } = string.Empty;
}