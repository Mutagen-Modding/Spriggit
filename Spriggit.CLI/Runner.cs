using System.IO.Abstractions;
using Noggog;
using Spriggit.CLI.Commands;
using Spriggit.Core;

namespace Spriggit.CLI;

public class Runner
{
    private Container GetContainer()
    {
        return new Container(new FileSystem(), null, null);
    }
    
    public async Task<int> Run(DeserializeCommand deserializeCommand)
    {
        SpriggitSource? source = null;
        if (!deserializeCommand.PackageName.IsNullOrWhitespace() ||
            !deserializeCommand.PackageVersion.IsNullOrWhitespace())
        {
            source = new SpriggitSource()
            {
                PackageName = deserializeCommand.PackageName,
                Version = deserializeCommand.PackageVersion,
            };
        }
        await GetContainer().Resolve().Value
            .Deserialize(
                spriggitPluginPath: deserializeCommand.InputPath,
                outputFile: deserializeCommand.OutputPath,
                source: source);
        return 0;
    }

    public async Task<int> Run(SerializeCommand serializeCommand)
    {
        await GetContainer().Resolve().Value
            .Serialize(
                bethesdaPluginPath: serializeCommand.InputPath,
                outputFolder: serializeCommand.OutputPath,
                meta: new SpriggitMeta(
                    new SpriggitSource()
                    {
                        PackageName = serializeCommand.PackageName,
                        Version = serializeCommand.PackageVersion,
                    },
                    serializeCommand.GameRelease)); 
        return 0;
    }
}