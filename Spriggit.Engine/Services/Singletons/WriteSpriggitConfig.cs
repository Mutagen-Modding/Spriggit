using System.IO.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Noggog;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

public class WriteSpriggitConfig
{
    public static readonly JsonSerializerSettings JsonSettings = new()
    {
        Converters =
        {
            new StringEnumConverter(),
        }
    };
    
    public void Write(FilePath path, SpriggitMeta meta, IFileSystem fileSystem)
    {
        var serialize = new SpriggitFileSerialize(
            meta.Source.PackageName,
            meta.Source.Version,
            meta.Release,
            []);
        var str = JsonConvert.SerializeObject(serialize, Formatting.Indented, JsonSettings);
        fileSystem.File.WriteAllText(path, str);
    }
}