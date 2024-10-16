using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.Sort;

public class SortFallout4 : ISort
{
    private readonly IFileSystem _fileSystem;

    public SortFallout4(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public bool HasWorkToDo(
        ModPath path, 
        GameRelease release,
        DirectoryPath? dataFolder)
    {
        using var mod = Fallout4Mod.Create(release.ToFallout4Release())
            .FromPath(path)
            .WithFileSystem(_fileSystem)
            .Construct();
        if (VirtualMachineAdaptersHaveWorkToDo(mod)) return true;
        if (MorphGroupHasWorkToDo(mod)) return true;
        return false;
    }

    private bool MorphGroupHasWorkToDo(IFallout4ModDisposableGetter mod)
    {
        foreach (var race in mod
                     .EnumerateMajorRecords<IRaceGetter>()
                     .AsParallel())
        {
            var headData = race.HeadData;
            if (HeadDataHasWorkToDo(headData?.Male))
            {
                Console.WriteLine($"{race} Male Head Data sorting to be done.");
                return true;
            }
            if (HeadDataHasWorkToDo(headData?.Female))
            {
                Console.WriteLine($"{race} Female Head Data sorting to be done.");
                return true;
            }
        }

        return false;
    }

    private bool HeadDataHasWorkToDo(IHeadDataGetter? headData)
    {
        if (headData == null) return false;
        var names = headData.MorphGroups.Select(x => x.Name).ToArray();
        if (!names.SequenceEqual(names.OrderBy(x => x)))
        {
            return true;
        }

        return false;
    }

    private bool VirtualMachineAdaptersHaveWorkToDo(IFallout4ModDisposableGetter mod)
    {
        foreach (var hasVM in mod
                     .EnumerateMajorRecords<IHaveVirtualMachineAdapterGetter>()
                     .AsParallel())
        {
            if (VirtualMachineAdapterHasWorkToDo(hasVM))
            {
                Console.WriteLine($"{hasVM} Virtual Machine Adapter has sorting to be done.");
                return true;
            }
        }

        return false;
    }

    private bool VirtualMachineAdapterHasWorkToDo(IHaveVirtualMachineAdapterGetter hasVM)
    {
        if (hasVM.VirtualMachineAdapter is not {} vm) return false;
        foreach (var script in vm.Scripts)
        {
            if (HasOutOfOrderScript(script)) return true;
        }

        if (vm is IQuestAdapter questAdapter)
        {
            if (HasOutOfOrderScript(questAdapter.Script)) return true;
            foreach (var script in questAdapter.Aliases.SelectMany(x => x.Scripts))
            {
                if (HasOutOfOrderScript(script)) return true;
            }
        }

        if (vm is IPerkAdapterGetter perkAdapter)
        {
            if (HasOutOfOrderScript(perkAdapter.ScriptFragments?.Script)) return true;
        }

        if (vm is IPackageAdapterGetter packageAdapter)
        {
            if (HasOutOfOrderScript(packageAdapter.ScriptFragments?.Script)) return true;
        }

        return false;
    }

    private bool HasOutOfOrderScript(IScriptEntryGetter? scriptEntry)
    {
        if (scriptEntry == null) return false;
        var names = scriptEntry.Properties.Select(x => x.Name).ToArray();
        if (!names.SequenceEqual(names.OrderBy(x => x)))
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
        var mod = Fallout4Mod.Create(release.ToFallout4Release())
            .FromPath(path)
            .WithFileSystem(_fileSystem)
            .Mutable()
            .Construct();
        SortVirtualMachineAdapter(mod);
        SortMorphGroups(mod);

        foreach (var maj in mod.EnumerateMajorRecords())
        {
            maj.IsCompressed = false;
        }
        
        await mod.BeginWrite
            .ToPath(outputPath)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .WithFileSystem(_fileSystem)
            .NoModKeySync()
            .WriteAsync();
    }

    private void SortVirtualMachineAdapter(IFallout4Mod mod)
    {
        foreach (var hasVM in mod
                     .EnumerateMajorRecords<IHaveVirtualMachineAdapter>())
        {
            if (hasVM.VirtualMachineAdapter is not {} vm) continue;
            foreach (var script in vm.Scripts)
            {
                ProcessScript(script);
            }
            
            if (vm is IQuestAdapter questAdapter)
            {
                ProcessScript(questAdapter.Script);
                foreach (var script in questAdapter.Aliases.SelectMany(x => x.Scripts))
                {
                    ProcessScript(script);
                }
            }

            if (vm is IPerkAdapter perkAdapter)
            {
                ProcessScript(perkAdapter.ScriptFragments?.Script);
            }

            if (vm is IPackageAdapter packageAdapter)
            {
                ProcessScript(packageAdapter.ScriptFragments?.Script);
            }
        }
    }

    private void SortMorphGroups(IFallout4Mod mod)
    {
        foreach (var race in mod
                     .EnumerateMajorRecords<IRace>()
                     .AsParallel())
        {
            SortHeadDataMorphGroups(race.HeadData?.Male);
            SortHeadDataMorphGroups(race.HeadData?.Female);
        }
    }

    private void SortHeadDataMorphGroups(IHeadData? headData)
    {
        if (headData == null) return;
        headData.MorphGroups.SetTo(
            headData.MorphGroups.ToArray().OrderBy(x => x.Name));
    }

    private void ProcessScript(IScriptEntry? scriptEntry)
    {
        if (scriptEntry == null) return;
        scriptEntry.Properties.SetTo(
            scriptEntry.Properties.ToArray().OrderBy(x => x.Name));
    }

    private void ProcessScripts(IEnumerable<IScriptEntry> scriptEntries)
    {
        foreach (var scriptEntry in scriptEntries)
        {
            ProcessScript(scriptEntry);
        }
    }
}