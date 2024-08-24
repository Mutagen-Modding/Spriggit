using System.IO.Abstractions;
using Mutagen.Bethesda.Plugins;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Noggog;
using NuGet.Versioning;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

public class SpriggitExternalMetaPersister
{
    public const string OldFileName = "spriggit.meta";
    public const string FileName = "spriggit-meta.json";
    private static readonly NuGetVersion SwapVersion = new NuGetVersion(0, 25, 0);
    
    private readonly IFileSystem _fileSystem;
    
    public static readonly JsonSerializerSettings JsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Converters =
        {
            new StringEnumConverter(),
        }
    };

    public SpriggitExternalMetaPersister(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public void Persist(DirectoryPath spriggitFolder, SpriggitModKeyMeta meta)
    {
        var nugetVersion = NuGetVersion.Parse(meta.Source.Version);
        var fileName = nugetVersion < SwapVersion ? OldFileName : FileName;
        var path = Path.Combine(spriggitFolder, fileName);
        var serializeObj = new SpriggitModKeyMetaSerialize(
            meta.Source.PackageName,
            meta.Source.Version,
            meta.Release,
            meta.ModKey.FileName);
        var str = JsonConvert.SerializeObject(serializeObj, Formatting.Indented, JsonSettings);
        _fileSystem.File.WriteAllText(path, str);
    }

    public SpriggitModKeyMeta? TryParseEmbeddedMeta(DirectoryPath spriggitFolder)
    {
        var path = Path.Combine(spriggitFolder, FileName);
        if (!_fileSystem.File.Exists(path))
        {
            path = Path.Combine(spriggitFolder, OldFileName);
            if (!_fileSystem.File.Exists(path))
            {
                return null;
            }
        }
        var str = _fileSystem.File.ReadAllText(path);
        var embedded = JsonConvert.DeserializeObject<SpriggitModKeyMetaSerialize>(str, JsonSettings);
        if (embedded == null
            || embedded.PackageName.IsNullOrWhitespace()
            || embedded.Version.IsNullOrWhitespace()
            || embedded.Release == null
            || embedded.ModKey.IsNullOrWhitespace()
            || !ModKey.TryFromFileName(embedded.ModKey, out var modKey))
        {
            return null;
        }

        return new SpriggitModKeyMeta(
            new SpriggitSource()
            {
                PackageName = embedded.PackageName,
                Version = embedded.Version
            },
            embedded.Release.Value,
            modKey);
    }
}