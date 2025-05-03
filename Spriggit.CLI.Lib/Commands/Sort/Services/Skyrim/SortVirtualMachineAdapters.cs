using Mutagen.Bethesda.Skyrim;
using Noggog;
using Serilog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Skyrim;

public class SortVirtualMachineAdapters : ISortSomething<ISkyrimMod, ISkyrimModGetter>
{
    private readonly ILogger _logger;

    public SortVirtualMachineAdapters(ILogger logger)
    {
        _logger = logger;
    }
    
    public bool HasWorkToDo(ISkyrimModGetter mod)
    {
        foreach (var hasVM in mod
                     .EnumerateMajorRecords<IHaveVirtualMachineAdapterGetter>()
                     .AsParallel())
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

        if (vm is IQuestAdapter questAdapter)
        {
            foreach (var script in questAdapter.Aliases.SelectMany(x => x.Scripts))
            {
                if (HasOutOfOrderScript(script)) return true;
            }
        }

        return false;
    }

    private bool HasOutOfOrderScript(IScriptEntryGetter scriptEntry)
    {
        var names = scriptEntry.Properties.Select(x => x.Name).ToArray();
        if (!names.SequenceEqual(names.OrderBy(x => x)))
        {
            return true;
        }

        return false;
    }

    public void DoWork(ISkyrimMod mod)
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