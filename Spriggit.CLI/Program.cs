using System.Globalization;
using CommandLine;
using Spriggit.CLI.Lib;
using Spriggit.CLI.Lib.Commands;
using Spriggit.Engine.Services.Singletons;

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
            typeof(StandardizeCommand))
        .MapResult(
            async (DeserializeCommand deserialize) => await EngineRunner.Run(deserialize, null),
            async (SerializeCommand serialize) => await EngineRunner.Run(serialize, null),
            async (FormIDCollisionCommand formIdCollision) => await FormIDCollisionRunner.Run(formIdCollision),
            async (MergeVersionSyncerCommand versionSyncer) => await MergeVersionSyncerRunner.Run(versionSyncer),
            async (StandardizeCommand standardize) => await StandardizeRunner.Run(standardize),
            async _ => -1);
}
catch (Exception ex)
{
    Console.WriteLine("Error:");
    Console.WriteLine(ex);
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
    return -1;
}
