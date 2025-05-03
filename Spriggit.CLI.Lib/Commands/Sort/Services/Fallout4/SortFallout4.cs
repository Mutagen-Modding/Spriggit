using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using Spriggit.Core;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Fallout4;

public class SortFallout4 : ISort
{
    private readonly ISortSomething<IFallout4Mod, IFallout4ModGetter>[] _sorters;
    private readonly IFileSystem _fileSystem;

    public SortFallout4(
        IFileSystem fileSystem,
        ISortSomething<IFallout4Mod, IFallout4ModGetter>[] sorters)
    {
        _fileSystem = fileSystem;
        _sorters = sorters;
    }
    
    public bool HasWorkToDo(
        ModPath path, 
        GameRelease release,
        KeyedMasterStyle[] knownMasters,
        DirectoryPath? dataFolder)
    {
        using var mod = Fallout4Mod.Create(release.ToFallout4Release())
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
        var mod = Fallout4Mod.Create(release.ToFallout4Release())
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