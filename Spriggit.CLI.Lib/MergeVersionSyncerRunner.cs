using System.IO.Abstractions;
using Spriggit.CLI.Commands;
using Spriggit.Engine;

namespace Spriggit.CLI;

public static class MergeVersionSyncerRunner
{
    private static MergeVersionSyncerContainer GetContainer(DebugState debugState)
    {
        return new MergeVersionSyncerContainer(new FileSystem(), debugState, LoggerSetup.Logger);
    }
    
    public static async Task<int> Run(MergeVersionSyncerCommand command)
    {
        LoggerSetup.Logger.Information("Command to handle FormID Collisions");

        await GetContainer(new DebugState { ClearNugetSources = command.Debug })
            .Resolve().Value
            .DetectAndFix(
                spriggitModPath: command.SpriggitPath); 
        return 0;
    }
}