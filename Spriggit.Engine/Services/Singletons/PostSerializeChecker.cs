using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Headers;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

public class PostSerializeChecker
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IWorkDropoff _workDropoff;
    private readonly ICreateStream? _createStream;

    public PostSerializeChecker(
        ILogger logger,
        IFileSystem fileSystem,
        IWorkDropoff? workDropoff,
        ICreateStream? createStream)
    {
        _logger = logger;
        _fileSystem = fileSystem;
        _workDropoff = workDropoff.GetOrFallback(() => InlineWorkDropoff.Instance);
        _createStream = createStream;
    }

    public async Task Check(
        ModPath mod,
        GameRelease release,
        DirectoryPath spriggit,
        DirectoryPath? dataPath,
        KnownMaster[] knownMasters,
        IEngineEntryPoint entryPt,
        CancellationToken cancel)
    {
        _logger.Information("Running basic correctness sanity checks:");
        _logger.Information($"  Path: {mod}");
        _logger.Information($"  Package: {entryPt.Package}");
        _logger.Information($"  Release: {release}");
        _logger.Information($"  Data Folder: {dataPath}");
        _logger.Information($"  Spriggit Folder: {spriggit}");
        var newSummaryTask = _workDropoff.EnqueueAndWait(async () =>
        {
            using var tmp = TempFolder.Factory(fileSystem: _fileSystem);
            var outPath = Path.Combine(tmp.Dir, mod.ModKey.FileName);
            await entryPt.Deserialize(
                inputPath: spriggit,
                outputPath: outPath,
                dataPath: dataPath,
                knownMasters: knownMasters,
                fileSystem: _fileSystem,
                workDropoff: _workDropoff,
                streamCreator: _createStream,
                cancel: cancel);
            return GetSummaryFor(outPath, release, dataPath, knownMasters);
        });
        var origSummaryTask = _workDropoff.EnqueueAndWait(() => GetSummaryFor(mod, release, dataPath, knownMasters));

        var newSummary = await newSummaryTask;
        var origSummary = await origSummaryTask;

        foreach (var id in origSummary.RecordIDs)
        {
            origSummary.RecordIDs.Remove(id);
            newSummary.RecordIDs.Remove(id);
        }

        if (newSummary.RecordIDs.Count > 0 || origSummary.RecordIDs.Count > 0)
        {
            _logger.Error("Mods had differing Record IDs:");
            if (origSummary.RecordIDs.Count > 0)
            {
                _logger.Error("  Original Mod:");
                foreach (var id in origSummary.RecordIDs)
                {
                    _logger.Error($"    {id}");
                }   
            }
            if (origSummary.RecordIDs.Count > 0)
            {
                _logger.Error("  Serialized Mod:");
                foreach (var id in newSummary.RecordIDs)
                {
                    _logger.Error($"    {id}");
                }   
            }

            throw new DataMisalignedException();
        }

        if (newSummary.RecordCount != origSummary.RecordCount)
        {
            _logger.Error($"Mods had differing record counts {newSummary.RecordCount} != {origSummary.RecordCount}");
            throw new DataMisalignedException();
        }
    }

    private class Summary
    {
        public int RecordCount;
        public HashSet<FormLinkInformation> RecordIDs = new();
    }

    private Summary GetSummaryFor(ModPath path, GameRelease release, DirectoryPath? dataDirectoryPath, KnownMaster[] knownMasters)
    {
        // ToDo
        // Eventually use generic read builder
        Cache<IModMasterStyledGetter, ModKey>? masterFlagsLookup = null;
        if (GameConstants.Get(release).SeparateMasterLoadOrders)
        {
            var header = ModHeaderFrame.FromPath(path, release, _fileSystem);
            masterFlagsLookup = new Cache<IModMasterStyledGetter, ModKey>(x => x.ModKey);
            var knownMastersLookup = knownMasters.ToDictionary(x => x.ModKey, x => x);
            foreach (var master in header.Masters(path.ModKey).Select(x => x.Master))
            {
                if (knownMastersLookup.TryGetValue(master, out var known))
                {
                    masterFlagsLookup.Add(new KeyedMasterStyle(known.ModKey, known.Style));
                }
                else
                {
                    if (dataDirectoryPath == null)
                    {
                        throw new ArgumentNullException(nameof(dataDirectoryPath), "Data directory was not set");
                    }

                    var otherPath = Path.Combine(dataDirectoryPath, master.FileName);
                    var otherHeader = ModHeaderFrame.FromPath(otherPath, release, _fileSystem);
                    masterFlagsLookup.Add(new KeyedMasterStyle(master, otherHeader.MasterStyle));
                }
            }
        }
        
        using var mod = ModInstantiator.ImportGetter(path, release, new BinaryReadParameters()
        {
            FileSystem = _fileSystem,
            MasterFlagsLookup = masterFlagsLookup
        });

        var summary = new Summary();

        foreach (var rec in mod.EnumerateMajorRecords())
        {
            summary.RecordCount++;
            summary.RecordIDs.Add(FormLinkInformation.Factory(rec));
        }

        return summary;
    }
}