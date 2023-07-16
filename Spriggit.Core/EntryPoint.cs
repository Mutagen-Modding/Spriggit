using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using Noggog.IO;
using Noggog.WorkEngine;

namespace Spriggit.Core;

public interface IEntryPoint<TMod, TModGetter> : IEntryPoint
    where TMod : class, TModGetter, IMod
    where TModGetter : class, IModGetter
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

    public Task<TMod> Deserialize(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        CancellationToken cancel);
}

public interface IEntryPoint
{
    public Task<SpriggitMeta?> TryGetMetaInfo(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        CancellationToken cancel);
}
