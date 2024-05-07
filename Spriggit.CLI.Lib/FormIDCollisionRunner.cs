using System.IO.Abstractions;
using Spriggit.CLI.Commands;
using Spriggit.Engine;

namespace Spriggit.CLI;

public static class FormIDCollisionRunner
{
    private static FormIDCollisionContainer GetContainer(DebugState debugState)
    {
        return new FormIDCollisionContainer(new FileSystem(), debugState, LoggerSetup.Logger);
    }
    
    public static async Task<int> Run(FormIDCollisionCommand formIdCollisionCommand)
    {
        LoggerSetup.Logger.Information("Command to handle FormID Collisions");

        await GetContainer(new DebugState { ClearNugetSources = formIdCollisionCommand.Debug })
            .Resolve().Value
            .DetectAndFix(
                spriggitModPath: formIdCollisionCommand.SpriggitPath); 
        return 0;
    }
}