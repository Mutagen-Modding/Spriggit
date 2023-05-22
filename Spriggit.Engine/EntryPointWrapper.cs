using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Serialization.Streams;
using Noggog;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Engine;

public class EntryPointWrapper : IEntryPoint<IMod, IModGetter>
{
    private readonly SerializeDelegate _serializeDelegate;
    private readonly DeserializeDelegate _deserializeDelegate;
    private readonly DeserializeSpriggitSourceDelegate _spriggitSourceDelegate;

    private delegate Task SerializeDelegate(
        ModPath modPath, 
        DirectoryPath dir,
        GameRelease release,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator,
        SpriggitSource meta);
    private delegate Task<IMod> DeserializeDelegate(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, 
        ICreateStream? streamCreator);
    private delegate Task<SpriggitMeta?> DeserializeSpriggitSourceDelegate(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem, 
        ICreateStream? streamCreator);
    
    private EntryPointWrapper(
        SerializeDelegate serializeDelegate,
        DeserializeDelegate deserializeDelegate,
        DeserializeSpriggitSourceDelegate spriggitSourceDelegate)
    {
        _serializeDelegate = serializeDelegate;
        _deserializeDelegate = deserializeDelegate;
        _spriggitSourceDelegate = spriggitSourceDelegate;
    }
    
    public static EntryPointWrapper Wrap<TMod, TModGetter>(IEntryPoint<TMod, TModGetter> entryPoint)
        where TMod : class, TModGetter, IMod
        where TModGetter : class, IModGetter
    {
        return new EntryPointWrapper(
            async (m, d, r, w, f, s, meta) => await entryPoint.Serialize(m, d, r, w, f, s, meta),
            async (i, w, f, s) => await entryPoint.Deserialize(i, w, f, s),
            async (i, w, f, s) => await entryPoint.TryGetMetaInfo(i, w, f, s));
    }

    public async Task Serialize(
        ModPath modPath, 
        DirectoryPath dir,
        GameRelease release,
        IWorkDropoff? workDropoff, IFileSystem? fileSystem,
        ICreateStream? streamCreator, SpriggitSource meta)
    {
        await _serializeDelegate(modPath, dir, release, workDropoff, fileSystem, streamCreator, meta);
    }

    public async Task<IMod> Deserialize(
        string inputPath,
        IWorkDropoff? workDropoff,
        IFileSystem? fileSystem,
        ICreateStream? streamCreator)
    {
        return await _deserializeDelegate(inputPath, workDropoff, fileSystem, streamCreator);
    }

    public async Task<SpriggitMeta?> TryGetMetaInfo(string inputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator)
    {
        return await _spriggitSourceDelegate(inputPath, workDropoff, fileSystem, streamCreator);
    }
}