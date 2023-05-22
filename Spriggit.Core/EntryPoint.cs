using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Serialization.Streams;
using Noggog;
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
        SpriggitSource meta);

    public Task<TMod> Deserialize(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator);
}

public interface IEntryPoint
{
    public Task<SpriggitMeta?> TryGetMetaInfo(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator);
}
