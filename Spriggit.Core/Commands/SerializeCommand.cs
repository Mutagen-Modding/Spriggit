﻿using CommandLine;
using Mutagen.Bethesda;
using Noggog;

namespace Spriggit.Core.Commands;

[Verb("serialize", aliases: new[] { "convert-from-plugin" }, HelpText = "Converts a binary Bethesda plugin to a text based representation")]
public class SerializeCommand
{
    [Option('i', "InputPath", 
        HelpText = "Path to the Bethesda plugin",
        Required = true)]
    public FilePath InputPath { get; set; } = string.Empty;
    
    [Option('o', "OutputPath",
        HelpText = "Dedicated folder to export mod as its text representation",
        Required = true)]
    public DirectoryPath OutputPath { get; set; } = string.Empty;
    
    [Option('g', "GameRelease",
        HelpText = "Game release that the plugin is related to",
        Required = false)]
    public GameRelease? GameRelease { get; set; }

    [Option('p', "PackageName",
        HelpText = "Spriggit serialization nuget package name to use for conversion",
        Required = false)]
    public string PackageName { get; set; } = string.Empty;

    [Option('v', "PackageVersion",
        HelpText = "Spriggit serialization nuget package version to use for conversion",
        Required = false)]
    public string PackageVersion { get; set; } = string.Empty;

    [Option('t', "Threads",
        HelpText = "Maximum number of threads to use",
        Required = false)]
    public byte? Threads { get; set; }

    [Option("Debug",
        HelpText = "Set up for debug mode, including resetting nuget caches",
        Required = false)]
    public bool Debug { get; set; }

    [Option('m', "ModKey",
        HelpText = "ModKey override.  If left blank, file name will be used",
        Required = false)]
    public string? ModKey { get; set; }

    [Option('c', "Check",
        HelpText = "Checks some basic correctness after serialization.  Not extensive.",
        Required = false)]
    public bool Check { get; set; } = true;

    [Option('d', "DataFolder",
        HelpText = "Path to the data folder to look to for mod files.  (Only required for separated master games, like Starfield)",
        Required = false)]
    public string? DataFolder { get; set; }

    [Option('u', "ErrorOnUnknown",
        HelpText = "Whether to error on unknown fields.  Liklihood of data loss is higher if turned off.",
        Required = false)]
    public bool? ErrorOnUnknown { get; set; }
}