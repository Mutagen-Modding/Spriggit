using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.Sort;

public class SortStarfield : ISort
{
    private readonly IFileSystem _fileSystem;

    public SortStarfield(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public bool HasWorkToDo(
        ModPath path,
        GameRelease release,
        DirectoryPath? dataFolder)
    {
        using var mod = StarfieldMod.Create(release.ToStarfieldRelease())
            .FromPath(path)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .WithFileSystem(_fileSystem)
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

            if (hasVM.VirtualMachineAdapter is IVirtualMachineAdapterIndexedGetter indexedAdapter)
            {
                if (HasOutOfOrderScript(indexedAdapter.ScriptFragments?.Script)) return true;
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

            if (hasVM.VirtualMachineAdapter is ISceneAdapterGetter sceneAdapter)
            {
                if (HasOutOfOrderScript(sceneAdapter.ScriptFragments?.Script)) return true;
            }

            if (hasVM.VirtualMachineAdapter is IDialogResponsesAdapterGetter dialAdapter)
            {
                if (HasOutOfOrderScript(dialAdapter.ScriptFragments?.Script)) return true;
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

        foreach (var prop in scriptEntry.Properties.OfType<IScriptStructPropertyGetter>())
        {
            foreach (var memb in prop.Members)
            {
                if (HasOutOfOrderScript(memb)) return true;
            }
        }

        return false;
    }

    public async Task Run(
        ModPath path, 
        GameRelease release, 
        ModPath outputPath,
        DirectoryPath? dataFolder)
    {
        var mod = StarfieldMod.Create(release.ToStarfieldRelease())
            .FromPath(path)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .Mutable()
            .WithFileSystem(_fileSystem)
            .Construct();
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