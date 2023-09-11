using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;

namespace Spriggit.Core;

public interface IEntryPoint
{
    protected static readonly BinaryWriteParameters NoCheckWriteParameters = new()
    {
        ModKey = ModKeyOption.NoCheck,
        MastersListContent = MastersListContentOption.NoCheck,
        RecordCount = RecordCountOption.NoCheck,
        MastersListOrdering = MastersListOrderingOption.NoCheck,
        NextFormID = NextFormIDOption.NoCheck,
        FormIDUniqueness = FormIDUniquenessOption.NoCheck,
        MasterFlag = MasterFlagOption.NoCheck,
        LightMasterLimit = LightMasterLimitOption.NoCheck,
        CleanNulls = false
    };

    public Task Serialize(
        ModPath modPath,
        DirectoryPath outputDir,
        GameRelease release,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        SpriggitSource meta,
        CancellationToken cancel);

    public Task Deserialize(
        string inputPath,
        string outputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        CancellationToken cancel);
    
    public Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        CancellationToken cancel);
}
