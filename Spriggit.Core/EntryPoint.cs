using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;

namespace Spriggit.Core;

public interface IEntryPoint
{
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
