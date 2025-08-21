using Mutagen.Bethesda.Fallout4;
using Noggog;
using Serilog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Fallout4;

public class SortVirtualMachineAdapters : ISortSomething<IFallout4Mod, IFallout4ModGetter>
{
    private readonly ILogger _logger;

    public SortVirtualMachineAdapters(ILogger logger)
    {
        _logger = logger;
    }
    
    public bool HasWorkToDo(IFallout4ModGetter mod)
    {
        foreach (var hasVM in mod
                     .EnumerateMajorRecords<IHaveVirtualMachineAdapterGetter>())
        {
            if (VirtualMachineAdapterHasWorkToDo(hasVM))
            {
                _logger.Information($"{hasVM} Virtual Machine Adapter has sorting to be done.");
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

        if (vm is IVirtualMachineAdapterIndexedGetter indexedAdapter)
        {
            if (HasOutOfOrderScript(indexedAdapter.ScriptFragments?.Script)) return true;
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
    
    public void DoWork(IFallout4Mod mod)
    {
        foreach (var hasVM in mod
                     .EnumerateMajorRecords<IHaveVirtualMachineAdapter>())
        {
            if (hasVM.VirtualMachineAdapter is not {} vm) continue;
            
            foreach (var script in vm.Scripts)
            {
                ProcessScript(script);
            }
            
            if (hasVM.VirtualMachineAdapter is IVirtualMachineAdapterIndexed indexedAdapter)
            {
                if (indexedAdapter.ScriptFragments is { } frags)
                {
                    ProcessScript(frags.Script);
                }
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
}