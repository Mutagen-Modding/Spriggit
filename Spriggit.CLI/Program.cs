using System.Globalization;
using CommandLine;
using Spriggit.CLI.Lib;
using Spriggit.CLI.Lib.Commands;
using Spriggit.CLI.Lib.Commands.FormIDCollision;
using Spriggit.CLI.Lib.Commands.MergeVersionSyncer;
using Spriggit.CLI.Lib.Commands.Standardize;
using Spriggit.CLI.Lib.Commands.UpgradeTargetSpriggitVersionCommand;
using Spriggit.Core;
using Spriggit.Core.Commands;

try
{
    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

    var vers = new CurrentVersionsProvider();
    Console.WriteLine($"Spriggit version {vers.SpriggitVersion}");
    Console.WriteLine(string.Join(' ', args));

    return await Parser.Default.ParseArguments(
            args,
            typeof(DeserializeCommand),
            typeof(SerializeCommand),
            typeof(FormIDCollisionCommand),
            typeof(MergeVersionSyncerCommand),
            typeof(StandardizeCommand),
            typeof(UpgradeTargetSpriggitVersionCommand))
        .MapResult(
            async (DeserializeCommand cmd) => await EngineRunner.Run(cmd, null),
            async (SerializeCommand cmd) => await EngineRunner.Run(cmd, null),
            async (FormIDCollisionCommand cmd) => await FormIDCollisionRunner.Run(cmd),
            async (MergeVersionSyncerCommand cmd) => await MergeVersionSyncerRunner.Run(cmd),
            async (StandardizeCommand cmd) => await StandardizeRunner.Run(cmd),
            async (UpgradeTargetSpriggitVersionCommand cmd) => await UpgradeTargetSpriggitVersionRunner.Run(cmd),
            async _ => 1);
}
catch (Exception ex)
{
    Console.WriteLine("Error:");
    Console.WriteLine(ex);
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
    return -1;
}
