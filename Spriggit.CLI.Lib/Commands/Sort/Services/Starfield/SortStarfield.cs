using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Starfield;
using Noggog;
using Serilog;
using Spriggit.Core;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;

public class SortStarfield : ISort
{
    private readonly ILogger _logger;
    private readonly ISortSomething<IStarfieldMod, IStarfieldModGetter>[] _sorters;
    private readonly IFileSystem _fileSystem;

    public SortStarfield(
        IFileSystem fileSystem, 
        ILogger logger,
        ISortSomething<IStarfieldMod, IStarfieldModGetter>[] sorters)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _sorters = sorters;
    }
    
    public bool HasWorkToDo(
        ModPath path,
        GameRelease release,
        KeyedMasterStyle[] knownMasters,
        DirectoryPath? dataFolder)
    {
        using var mod = StarfieldMod.Create(release.ToStarfieldRelease())
            .FromPath(path)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .WithKnownMasters(knownMasters)
            .WithFileSystem(_fileSystem)
            .Construct();

        return _sorters
            .Any(s => s.HasWorkToDo(mod));
    }

    public async Task Run(
        ModPath path, 
        GameRelease release, 
        ModPath outputPath,
        KeyedMasterStyle[] knownMasters,
        DirectoryPath? dataFolder)
    {
        var mod = StarfieldMod.Create(release.ToStarfieldRelease())
            .FromPath(path)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .WithKnownMasters(knownMasters)
            .Mutable()
            .WithFileSystem(_fileSystem)
            .Construct();
        foreach (var sorter in _sorters)
        {
            sorter.DoWork(mod);
        }

        outputPath.Path.Directory?.Create(_fileSystem);
        await mod.BeginWrite
            .ToPath(outputPath)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .WithKnownMasters(knownMasters)
            .WithFileSystem(_fileSystem)
            .AddNonOpinionatedWriteOptions()
            .WriteAsync();
    }
}