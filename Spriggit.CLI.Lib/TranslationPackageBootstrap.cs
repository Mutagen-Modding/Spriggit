using CommandLine;
using Spriggit.CLI.Lib.Commands;
using Spriggit.Core.Commands;
using Spriggit.Engine;

namespace Spriggit.CLI.Lib;

public static class TranslationPackageBootstrap
{
    public static async Task<int> Run(
        string[] args,
        IEngineEntryPoint entryPoint)
    {
        return await Parser.Default.ParseArguments(args, typeof(DeserializeCommand), typeof(SerializeCommand))
            .MapResult(
                async (DeserializeCommand deserialize) => await EngineRunner.Run(deserialize, entryPoint),
                async (SerializeCommand serialize) => await EngineRunner.Run(serialize, entryPoint),
                async _ => -1);
    }
}