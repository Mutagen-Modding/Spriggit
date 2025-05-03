using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Serilog;
using Spriggit.Core;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Skyrim;

public class SortSkyrim : ISort
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly ISortSomething<ISkyrimMod, ISkyrimModGetter>[] _sorters;

    public SortSkyrim(
        ILogger logger,
        IFileSystem fileSystem,
        ISortSomething<ISkyrimMod, ISkyrimModGetter>[] sorters)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _sorters = sorters;
    }
    
    public bool HasWorkToDo(
        ModPath path,
        GameRelease release,
        KeyedMasterStyle[] knownMasters,
        DirectoryPath? dataFolder)
    {
        using var mod = SkyrimMod.Create(release.ToSkyrimRelease())
            .FromPath(path)
            .WithFileSystem(_fileSystem)
            .Construct();

        return _sorters.AsParallel()
            .Any(s => s.HasWorkToDo(mod));
    }

    public async Task Run(
        ModPath path,
        GameRelease release, 
        ModPath outputPath,
        KeyedMasterStyle[] knownMasters,
        DirectoryPath? dataFolder)
    {
        var mod = SkyrimMod.Create(release.ToSkyrimRelease())
            .FromPath(path)
            .WithFileSystem(_fileSystem)
            .Mutable()
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
            .WithFileSystem(_fileSystem)
            .AddNonOpinionatedWriteOptions()
            .WriteAsync();
    }
}