using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

public class LocalizeEnforcer
{
    private readonly IFileSystem _fileSystem;

    public LocalizeEnforcer(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public void Localize(
        bool localize,
        ModPath modPath,
        GameRelease release,
        KnownMaster[]? knownMasters)
    {
        KeyedMasterStyle[]? keyedMasterStyles = knownMasters?
            .Select(x => new KeyedMasterStyle(x.ModKey, x.Style))
            .ToArray();
        LoadOrder<IModMasterStyledGetter>? masterFlagsLookup =
            keyedMasterStyles == null ? null : new LoadOrder<IModMasterStyledGetter>(keyedMasterStyles);

        var mod = ModInstantiator.ImportSetter(modPath, release, BinaryReadParameters.Default with
        {
            FileSystem = _fileSystem,
            MasterFlagsLookup = masterFlagsLookup
        });
        if (localize == mod.UsingLocalization) return;
        if (localize && !mod.CanUseLocalization)
        {
            throw new ArgumentException($"Could not localize a GameRelease which does not support the concept: {release}");
        }

        mod.UsingLocalization = localize;
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = _fileSystem,
            MasterFlagsLookup = masterFlagsLookup
        });
    }
}