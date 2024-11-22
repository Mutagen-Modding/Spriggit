using System.IO.Abstractions;
using Noggog.WorkEngine;
using Spriggit.CLI.Lib.Commands;
using Spriggit.Engine;
using Spriggit.Engine.Services.Singletons;

namespace Spriggit.CLI.Lib;

public static class EngineRunner
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
        // var workDropoff = new WorkDropoff();
        // var worker = new WorkConsumer(new NumWorkThreadsConstant(threads), workDropoff);
        // worker.Start();
        // return workDropoff;
    }
    
    private static EngineContainer GetContainer(DebugState debugState, byte? numThreads)
    {
        return new EngineContainer(new FileSystem(), GetWorkDropoff(numThreads), null, debugState, LoggerSetup.Logger);
    }
    
    public static async Task<int> Run(DeserializeCommand deserializeCommand, IEngineEntryPoint? entryPoint)
    {
        LoggerSetup.Logger.Information("Command to deserialize");

        var source = SerializeCommandMetaExtractor.ExtractSpriggitSource(deserializeCommand);
        
        await GetContainer(new DebugState { ClearNugetSources = deserializeCommand.Debug }, deserializeCommand.Threads)
            .Resolve().Value
            .Deserialize(
                spriggitPluginPath: deserializeCommand.InputPath,
                outputFile: deserializeCommand.OutputPath,
                dataPath: deserializeCommand.DataFolder,
                source: source,
                entryPt: entryPoint,
                localize: deserializeCommand.Localized,
                backupDays: deserializeCommand.BackupDays,
                cancel: CancellationToken.None);
        return 0;
    }

    public static async Task<int> Run(SerializeCommand serializeCommand, IEngineEntryPoint? entryPoint)
    {
        LoggerSetup.Logger.Information("Command to serialize");

        var meta = SerializeCommandMetaExtractor.ExtractSpriggitMeta(serializeCommand);

        var modPath = SerializeCommandMetaExtractor.ExtractModPath(serializeCommand);

        await GetContainer(new DebugState { ClearNugetSources = serializeCommand.Debug }, serializeCommand.Threads)
            .Resolve().Value
            .Serialize(
                bethesdaPluginPath: modPath,
                outputFolder: serializeCommand.OutputPath,
                dataPath: serializeCommand.DataFolder,
                entryPt: entryPoint,
                postSerializeChecks: serializeCommand.Check,
                meta: meta,
                cancel: CancellationToken.None); 
        return 0;
    }
}