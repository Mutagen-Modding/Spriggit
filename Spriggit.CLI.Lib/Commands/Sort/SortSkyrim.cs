using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.Sort;

public class SortSkyrim : ISort
{
    private readonly IFileSystem _fileSystem;

    public SortSkyrim(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public bool HasWorkToDo(
        ModPath path,
        GameRelease release,
        DirectoryPath? dataFolder)
    {
        using var mod = SkyrimMod.Create(release.ToSkyrimRelease())
            .FromPath(path)
            .WithFileSystem(_fileSystem)
            .Construct();
        if (HasVirtualMachineAdapterWorkToDo(mod)) return true;

        return false;
    }

    private bool HasVirtualMachineAdapterWorkToDo(ISkyrimModDisposableGetter mod)
    {
        foreach (var hasVM in mod
                     .EnumerateMajorRecords<IHaveVirtualMachineAdapterGetter>()
                     .AsParallel())
        {
            if (hasVM.VirtualMachineAdapter == null) continue;
            foreach (var script in hasVM.VirtualMachineAdapter.Scripts)
            {
                if (HasOutOfOrderScript(script)) return true;
            }

            if (hasVM.VirtualMachineAdapter is IQuestAdapter questAdapter)
            {
                foreach (var script in questAdapter.Aliases.SelectMany(x => x.Scripts))
                {
                    if (HasOutOfOrderScript(script)) return true;
                }
            }
        }

        return false;
    }

    private bool HasOutOfOrderScript(IScriptEntryGetter scriptEntry)
    {
        if (!scriptEntry.Properties.Select(x => x.Name)
                .SequenceEqual(scriptEntry.Properties.OrderBy(x => x.Name).Select(x => x.Name)))
        {
            return true;
        }

        return false;
    }

    public async Task Run(
        ModPath path,
        GameRelease release, 
        ModPath outputPath,
        DirectoryPath? dataFolder)
    {
        var mod = SkyrimMod.Create(release.ToSkyrimRelease())
            .FromPath(path)
            .WithFileSystem(_fileSystem)
            .Mutable()
            .Construct();
        SortVirtualMachineAdapter(mod);

        foreach (var maj in mod.EnumerateMajorRecords())
        {
            maj.IsCompressed = false;
        }
        
        await mod.BeginWrite
            .ToPath(outputPath)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .NoModKeySync()
            .WithFileSystem(_fileSystem)
            .WriteAsync();
    }

    private void SortVirtualMachineAdapter(ISkyrimMod mod)
    {
        foreach (var hasVM in mod
                     .EnumerateMajorRecords<IHaveVirtualMachineAdapter>())
        {
            if (hasVM.VirtualMachineAdapter == null) continue;
            foreach (var script in hasVM.VirtualMachineAdapter.Scripts)
            {
                ProcessScript(script);
            }
            
            if (hasVM.VirtualMachineAdapter is IQuestAdapter questAdapter)
            {
                foreach (var script in questAdapter.Aliases.SelectMany(x => x.Scripts))
                {
                    ProcessScript(script);
                }
            }
        }
    }

    private void ProcessScript(IScriptEntry scriptEntry)
    {
        scriptEntry.Properties.SetTo(
            scriptEntry.Properties.ToArray().OrderBy(x => x.Name));
    }
}