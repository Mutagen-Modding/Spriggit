using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.Sort;

public class SortFallout4 : ISort
{
    public bool HasWorkToDo(
        ModPath path, 
        GameRelease release,
        DirectoryPath? dataFolder)
    {
        using var mod = Fallout4Mod.Create(release.ToFallout4Release())
            .FromPath(path)
            .Construct();
        if (VirtualMachineAdapterHasWorkToDo(mod)) return true;
        return false;
    }

    private bool VirtualMachineAdapterHasWorkToDo(IFallout4ModDisposableGetter mod)
    {
        foreach (var hasVM in mod
                     .EnumerateMajorRecords<IHaveVirtualMachineAdapterGetter>()
                     .AsParallel())
        {
            if (hasVM.VirtualMachineAdapter is not {} vm) continue;
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