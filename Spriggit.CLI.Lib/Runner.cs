using System.IO.Abstractions;
using Noggog;
using Noggog.WorkEngine;
using Spriggit.CLI.Commands;
using Spriggit.Core;
using Spriggit.Engine;

namespace Spriggit.CLI;

public static class Runner
{
    private static IWorkDropoff GetWorkDropoff(int? threads)
    {
        if (threads == null || threads >= byte.MaxValue)
        {
            return new ParallelWorkDropoff();
        }
        
        if (threads <= 1)
        {
            return new InlineWorkDropoff();
        }

        throw new NotImplementedException("Specific desired threads not yet implemented.");
        // This setup throws a stackoverflow.  Need to research/improve
        var workDropoff = new WorkDropoff();
        var worker = new WorkConsumer(new NumWorkThreadsConstant(threads), workDropoff);
        worker.Start();
        return workDropoff;
    }
    
    private static Container GetContainer(DebugState debugState, byte? numThreads)
    {
        return new Container(new FileSystem(), GetWorkDropoff(numThreads), null, debugState, LoggerSetup.Logger);
    }
    
    public static async Task<int> Run(DeserializeCommand deserializeCommand)
    {
        LoggerSetup.Logger.Information("Command to deserialize");
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
        await GetContainer(new DebugState { ClearNugetSources = deserializeCommand.Debug }, deserializeCommand.Threads)
            .Resolve().Value
            .Deserialize(
                spriggitPluginPath: deserializeCommand.InputPath,
                outputFile: deserializeCommand.OutputPath,
                source: source,
                cancel: CancellationToken.None);
        return 0;
    }

    public static async Task<int> Run(SerializeCommand serializeCommand)
    {
        LoggerSetup.Logger.Information("Command to serialize");
        await GetContainer(new DebugState { ClearNugetSources = serializeCommand.Debug }, serializeCommand.Threads)
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