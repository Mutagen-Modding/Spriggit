using Mutagen.Bethesda.Plugins;
using Noggog;
using Noggog.IO;
using Spriggit.Core;
using Spriggit.Engine;

namespace Spriggit.Engine.Services.Singletons;

public interface ISpriggitEngine
{
    Task Serialize(
        ModPath bethesdaPluginPath,
        DirectoryPath outputFolder,
        DirectoryPath? dataPath,
        bool postSerializeChecks,
        bool throwOnUnknown,
        IEngineEntryPoint? entryPt = default,
        SpriggitMeta? meta = default,
        CancellationToken? cancel = default);

    Task Deserialize(
        string spriggitPluginPath,
        FilePath outputFile,
        DirectoryPath? dataPath,
        uint backupDays,
        bool? localize,
        IEngineEntryPoint? entryPt = default,
        SpriggitSource? source = default,
        CancellationToken? cancel = default);
}