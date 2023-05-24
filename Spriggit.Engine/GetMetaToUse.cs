using System.Data;
using System.IO.Abstractions;
using Mutagen.Bethesda.Serialization.Streams;
using Noggog.WorkEngine;
using Spriggit.Core;

namespace Spriggit.Engine;

public class GetMetaToUse
{
    private readonly IFileSystem _fileSystem;
    private readonly IWorkDropoff? _workDropoff;
    private readonly ICreateStream? _createStream;
    private readonly GetDefaultEntryPoint _getDefaultEntryPoint;

    public GetMetaToUse(
        IFileSystem fileSystem,
        IWorkDropoff? workDropoff,
        ICreateStream? createStream,
        GetDefaultEntryPoint getDefaultEntryPoint)
    {
        _fileSystem = fileSystem;
        _workDropoff = workDropoff;
        _createStream = createStream;
        _getDefaultEntryPoint = getDefaultEntryPoint;
    }
    
    private SpriggitSource ConvertToSource(
        string packageName,
        string packageVersion)
    {
        return new SpriggitSource()
        {
            Version = packageVersion,
            PackageName = packageName
        };
    }
    
    public async Task<SpriggitMeta> Get(
        SpriggitSource? source,
        string spriggitPluginPath)
    {
        var entryPt = await _getDefaultEntryPoint.Get(spriggitPluginPath);
        var sourceInfo = await entryPt.TryGetMetaInfo(
            spriggitPluginPath,
            _workDropoff, 
            _fileSystem, 
            _createStream);
        if (sourceInfo == null)
        {
            throw new DataException($"Could not locate source info from {spriggitPluginPath}");
        }

        if (source != null)
        {
            return sourceInfo with
            {
                Source = source
            };
        }

        return sourceInfo;
    }
}