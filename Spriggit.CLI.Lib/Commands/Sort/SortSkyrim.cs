using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Serilog;
using Spriggit.Core;

namespace Spriggit.CLI.Lib.Commands.Sort;

public class SortSkyrim : ISort
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;

    public SortSkyrim(
        ILogger logger,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }
    
    public bool HasWorkToDo(
        ModPath path,
        GameRelease release,
        KeyedMasterStyle[] knownMasters,
        DirectoryPath? dataFolder)
    {
        using var mod = SkyrimMod.Create(release.ToSkyrimRelease())
            .FromPath(path)
            .WithFileSystem(_fileSystem)
            .Construct();
        if (VirtualMachineAdaptersHaveWorkToDo(mod)) return true;

        return false;
    }

    private bool VirtualMachineAdaptersHaveWorkToDo(ISkyrimModDisposableGetter mod)
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

    public async Task Run(
        ModPath path,
        GameRelease release, 
        ModPath outputPath,
        KeyedMasterStyle[] knownMasters,
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
        
        outputPath.Path.Directory?.Create(_fileSystem);
        await mod.BeginWrite
            .ToPath(outputPath)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .WithFileSystem(_fileSystem)
            .AddNonOpinionatedWriteOptions()
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