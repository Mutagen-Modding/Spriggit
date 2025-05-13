using System.Globalization;
using System.IO.Abstractions;
using CommandLine;
using Spriggit.Core;
using Spriggit.Core.Commands;
using Spriggit.Core.Services.Singletons;

namespace Spriggit.TranslationPackages;

public static class TranslationPackageRunner
{
    public static async Task<int> Run(
        string[] args,
        IEntryPoint entryPoint)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var vers = new CurrentVersionsProvider();
        Console.WriteLine($"Spriggit version {vers.SpriggitVersion}");
        Console.WriteLine(string.Join(' ', args));

        return await Parser.Default.ParseArguments(
                args, 
                typeof(DeserializeCommand),
                typeof(SerializeCommand))
            .MapResult(
                async (DeserializeCommand cmd) =>
                {
                    var spriggitFileLocator = new SpriggitFileLocatorContainer(new FileSystem(), LoggerSetup.Logger).Resolve().Value;
                    var spriggitFile = spriggitFileLocator.LocateAndParse(cmd.InputPath);

                    LoggerSetup.Logger.Information("Starting deserialization from {InputPath} to {OutputPath}", cmd.InputPath, cmd.OutputPath);
                    await entryPoint.Deserialize(
                        cmd.InputPath,
                        cmd.OutputPath,
                        cmd.DataFolder,
                        knownMasters: spriggitFile?.KnownMasters ?? [],
                        workDropoff: null,
                        fileSystem: null,
                        streamCreator: null,
                        CancellationToken.None);
                    LoggerSetup.Logger.Information("Finished deserializing");
                    return 0;
                },
                async (SerializeCommand cmd) =>
                {
                    var spriggitFileLocator = new SpriggitFileLocatorContainer(new FileSystem(), LoggerSetup.Logger).Resolve().Value;
                    var spriggitFile = spriggitFileLocator.LocateAndParse(cmd.OutputPath);
                    
                    var meta = SerializeCommandMetaExtractor.ExtractSpriggitMeta(cmd);
                    
                    if (meta == null && spriggitFile != null)
                    {
                        meta = spriggitFile.Meta;
                    }
                    
                    if (meta == null)
                    {
                        throw new NotSupportedException($"Could not locate meta to run with.  Either run serialize in with a {SpriggitFileLocator.ConfigFileName} file present, or specify at least GameRelease and PackageName");
                    }

                    var throwing = cmd.ErrorOnUnknown ?? true;
                    
                    LoggerSetup.Logger.Information("Throwing on unknown records: {Throwing}", throwing);

                    LoggerSetup.Logger.Information("Starting serialization from {InputPath} to {OutputPath}", cmd.InputPath, cmd.OutputPath);
                    await entryPoint.Serialize(
                        cmd.InputPath,
                        cmd.OutputPath,
                        cmd.DataFolder,
                        knownMasters: spriggitFile?.KnownMasters ?? [],
                        release: cmd.GameRelease ?? meta.Release,
                        workDropoff: null,
                        fileSystem: null,
                        streamCreator: null,
                        meta: meta.Source,
                        throwIfUnknown: throwing,
                        CancellationToken.None);
                    
                    LoggerSetup.Logger.Information("Finished serializing");
                    return 0;
                },
                async _ => -1);
    }
}