﻿using Mutagen.Bethesda.Starfield;
using Noggog;
using Serilog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;

public class SortVirtualMachineAdapters : ISortSomething<IStarfieldMod, IStarfieldModGetter>
{
    private readonly ILogger _logger;

    public SortVirtualMachineAdapters(ILogger logger)
    {
        _logger = logger;
    }
    
    public bool HasWorkToDo(IStarfieldModGetter mod)
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

        if (vm is ISceneAdapterGetter sceneAdapter)
        {
            if (HasOutOfOrderScript(sceneAdapter.ScriptFragments?.Script)) return true;
        }

        if (vm is IDialogResponsesAdapterGetter dialAdapter)
        {
            if (HasOutOfOrderScript(dialAdapter.ScriptFragments?.Script)) return true;
        }

        return false;
    }

    private bool HasOutOfOrderScript(IScriptEntryGetter? scriptEntry)
    {
        if (scriptEntry == null) return false;
        var props = scriptEntry.Properties.ToArray();
        var names = props.Select(x => x.Name).ToArray();
        if (!names.SequenceEqual(names.OrderBy(x => x)))
        {
            return true;
        }

        foreach (var prop in props.OfType<IScriptStructPropertyGetter>())
        {
            foreach (var memb in prop.Members)
            {
                if (HasOutOfOrderScript(memb)) return true;
            }
        }

        return false;
    }

    public void DoWork(IStarfieldMod mod)
    {
        foreach (var hasVM in mod
                     .EnumerateMajorRecords<IHaveVirtualMachineAdapter>())
        {
            if (hasVM.VirtualMachineAdapter == null) continue;
            foreach (var script in hasVM.VirtualMachineAdapter.Scripts)
            {
                ProcessScript(script);
            }
            
            if (hasVM.VirtualMachineAdapter is IVirtualMachineAdapterIndexed indexedAdapter)
            {
                ProcessScripts(indexedAdapter.Scripts);
                if (indexedAdapter.ScriptFragments is { } frags)
                {
                    ProcessScript(frags.Script);
                }
            }
            
            if (hasVM.VirtualMachineAdapter is IQuestAdapter questAdapter)
            {
                ProcessScript(questAdapter.Script);
                ProcessScripts(questAdapter.Scripts);
            }

            if (hasVM.VirtualMachineAdapter is IPerkAdapter perkAdapter)
            {
                ProcessScript(perkAdapter.ScriptFragments?.Script);
            }

            if (hasVM.VirtualMachineAdapter is IPackageAdapter packageAdapter)
            {
                ProcessScript(packageAdapter.ScriptFragments?.Script);
            }

            if (hasVM.VirtualMachineAdapter is ISceneAdapter sceneAdapter)
            {
                ProcessScript(sceneAdapter.ScriptFragments?.Script);
            }

            if (hasVM.VirtualMachineAdapter is IDialogResponsesAdapter dialAdapter)
            {
                ProcessScript(dialAdapter.ScriptFragments?.Script);
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