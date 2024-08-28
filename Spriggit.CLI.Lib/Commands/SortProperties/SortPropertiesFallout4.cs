using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Fallout4;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.SortProperties;

public class SortPropertiesFallout4 : ISortProperties
{
    public bool HasWorkToDo(
        ModPath path, 
        GameRelease release,
        DirectoryPath? dataFolder)
    {
        using var mod = Fallout4Mod.Create(release.ToFallout4Release())
            .FromPath(path)
            .Construct();
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
                if (HasOutOfOrderScript(questAdapter.Script)) return true;
                foreach (var script in questAdapter.Aliases.SelectMany(x => x.Scripts))
                {
                    if (HasOutOfOrderScript(script)) return true;
                }
            }

            if (hasVM.VirtualMachineAdapter is IPerkAdapterGetter perkAdapter)
            {
                if (HasOutOfOrderScript(perkAdapter.ScriptFragments?.Script)) return true;
            }

            if (hasVM.VirtualMachineAdapter is IPackageAdapterGetter packageAdapter)
            {
                if (HasOutOfOrderScript(packageAdapter.ScriptFragments?.Script)) return true;
            }
        }

        return false;
    }

    private bool HasOutOfOrderScript(IScriptEntryGetter? scriptEntry)
    {
        if (scriptEntry == null) return false;
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
        var mod = Fallout4Mod.Create(release.ToFallout4Release())
            .FromPath(path)
            .Mutable()
            .Construct();
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
                ProcessScript(questAdapter.Script);
                foreach (var script in questAdapter.Aliases.SelectMany(x => x.Scripts))
                {
                    ProcessScript(script);
                }
            }

            if (hasVM.VirtualMachineAdapter is IPerkAdapter perkAdapter)
            {
                ProcessScript(perkAdapter.ScriptFragments?.Script);
            }

            if (hasVM.VirtualMachineAdapter is IPackageAdapter packageAdapter)
            {
                ProcessScript(packageAdapter.ScriptFragments?.Script);
            }
        }
        await mod.BeginWrite
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .ToPath(outputPath)
            .NoModKeySync()
            .WriteAsync();
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