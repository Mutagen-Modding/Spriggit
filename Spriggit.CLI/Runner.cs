using System.IO.Abstractions;
using Noggog;
using Spriggit.CLI.Commands;
using Spriggit.Core;
using Spriggit.Engine;

namespace Spriggit.CLI;

public class Runner
{
    private Container GetContainer(DebugState debugState)
    {
        return new Container(new FileSystem(), null, null, debugState, LoggerSetup.Logger);
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
        await GetContainer(new()
            {
                ClearNugetSources = deserializeCommand.Debug
            })
            .Resolve().Value
            .Deserialize(
                spriggitPluginPath: deserializeCommand.InputPath,
                outputFile: deserializeCommand.OutputPath,
                source: source,
                cancel: CancellationToken.None);
        return 0;
    }

    public async Task<int> Run(SerializeCommand serializeCommand)
    {
        await GetContainer(new()
            {
                ClearNugetSources = serializeCommand.Debug
            })
            .Resolve().Value
            .Serialize(
                bethesdaPluginPath: serializeCommand.InputPath,
                outputFolder: serializeCommand.OutputPath,
                meta: new SpriggitMeta(
                    new SpriggitSource()
                    {
                        PackageName = serializeCommand.PackageName,
                        Version = serializeCommand.PackageVersion,
                    },
                    serializeCommand.GameRelease),
                cancel: CancellationToken.None); 
        return 0;
    }
}