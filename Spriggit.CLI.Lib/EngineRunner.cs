using System.IO.Abstractions;
using Noggog;
using Noggog.WorkEngine;
using Spriggit.CLI.Commands;
using Spriggit.Core;
using Spriggit.Engine;

namespace Spriggit.CLI;

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
        var workDropoff = new WorkDropoff();
        var worker = new WorkConsumer(new NumWorkThreadsConstant(threads), workDropoff);
        worker.Start();
        return workDropoff;
    }
    
    private static EngineContainer GetContainer(DebugState debugState, byte? numThreads)
    {
        return new EngineContainer(new FileSystem(), GetWorkDropoff(numThreads), null, debugState, LoggerSetup.Logger);
    }
    
    public static async Task<int> Run(DeserializeCommand deserializeCommand)
    {
        LoggerSetup.Logger.Information("Command to deserialize");
        SpriggitSource? source = null;

        if (!deserializeCommand.PackageVersion.IsNullOrWhitespace() && deserializeCommand.PackageName.IsNullOrWhitespace())
        {
            throw new ArgumentException("Cannot specify PackageVersion without PackageName.");
        }
        
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
                backupDays: deserializeCommand.BackupDays,
                cancel: CancellationToken.None);
        return 0;
    }

    public static async Task<int> Run(SerializeCommand serializeCommand)
    {
        LoggerSetup.Logger.Information("Command to serialize");

        SpriggitMeta? meta;
        if (serializeCommand.GameRelease == null || serializeCommand.PackageName.IsNullOrWhitespace())
        {
            if (serializeCommand.PackageVersion != null)
            {
                throw new ArgumentException(
                    "Cannot specify version without also specifying GameRelease and PackageName");
            }
            meta = null;
        }
        else
        {
            meta = new SpriggitMeta(
                new SpriggitSource()
                {
                    PackageName = serializeCommand.PackageName,
                    Version = serializeCommand.PackageVersion
                },
                Release: serializeCommand.GameRelease.Value);
        }
        
        await GetContainer(new DebugState { ClearNugetSources = serializeCommand.Debug }, serializeCommand.Threads)
            .Resolve().Value
            .Serialize(
                bethesdaPluginPath: serializeCommand.InputPath,
                outputFolder: serializeCommand.OutputPath,
                meta: meta,
                cancel: CancellationToken.None); 
        return 0;
    }
}