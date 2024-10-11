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
        if (VirtualMachineAdapterHasWorkToDo(mod)) return true;
        if (RaceMorphGroupHasWorkToDo(mod)) return true;
        if (NpcsHaveWorkToDo(mod)) return true;

        return false;
    }

    private bool RaceMorphGroupHasWorkToDo(IStarfieldModDisposableGetter mod)
    {
        foreach (var race in mod
                     .EnumerateMajorRecords<IRaceGetter>()
                     .AsParallel())
        {
            var charGen = race.ChargenAndSkintones;
            if (ChargenHasWorkToDo(charGen?.Male)) return true;
            if (ChargenHasWorkToDo(charGen?.Female)) return true;
        }

        return false;
    }

    private bool NpcsHaveWorkToDo(IStarfieldModDisposableGetter mod)
    {
        foreach (var npc in mod
                     .EnumerateMajorRecords<INpcGetter>()
                     .AsParallel())
        {
            var morphGroupNames = npc.FaceMorphs.SelectMany(x => x.MorphGroups).Select(x => x.MorphGroup).ToArray();
            if (!morphGroupNames.SequenceEqual(morphGroupNames.OrderBy(x => x))) return true;
            var morphBlendNames = npc.MorphBlends.Select(x => x.BlendName).ToArray();
            if (!morphBlendNames.SequenceEqual(morphBlendNames.OrderBy(x => x))) return true;
        }

        return false;
    }

    private bool ChargenHasWorkToDo(IChargenAndSkintonesGetter? charGen)
    {
        if (charGen?.Chargen == null) return false;
        var names = charGen.Chargen.MorphGroups.Select(x => x.Name).ToArray();
        if (!names.SequenceEqual(names.OrderBy(x => x)))
        {
            return true;
        }

        return false;
    }

    private bool VirtualMachineAdapterHasWorkToDo(IStarfieldModDisposableGetter mod)
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
        SortVirtualMachineAdapter(mod);
        SortMorphGroups(mod);
        SortMorphBlends(mod);

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

    private void SortVirtualMachineAdapter(IStarfieldMod mod)
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

    private void SortMorphGroups(IStarfieldMod mod)
    {
        foreach (var race in mod
                     .EnumerateMajorRecords<IRace>()
                     .AsParallel())
        {
            SortChargenMorphGroups(race.ChargenAndSkintones?.Male?.Chargen);
            SortChargenMorphGroups(race.ChargenAndSkintones?.Female?.Chargen);
        }
        foreach (var npc in mod
                     .EnumerateMajorRecords<INpc>()
                     .AsParallel())
        {
            foreach (var faceMorph in npc.FaceMorphs)
            {
                SortNpcFaceMorphs(faceMorph);
            }
        }
    }

    private void SortMorphBlends(IStarfieldMod mod)
    {
        foreach (var npc in mod
                     .EnumerateMajorRecords<INpc>()
                     .AsParallel())
        {
            npc.MorphBlends.SetTo(
                npc.MorphBlends.ToArray().OrderBy(x => x.BlendName));
        }
    }

    private void SortNpcFaceMorphs(INpcFaceMorph npcFaceMorph)
    {
        npcFaceMorph.MorphGroups.SetTo(
            npcFaceMorph.MorphGroups.ToArray().OrderBy(x => x.MorphGroup));
    }

    private void SortChargenMorphGroups(IChargen? item)
    {
        if (item == null) return;
        item.MorphGroups.SetTo(
            item.MorphGroups.ToArray().OrderBy(x => x.Name));
    }
}