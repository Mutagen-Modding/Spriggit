using System.IO.Abstractions;
using Newtonsoft.Json;
using Noggog;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine;

public class SpriggitMetaLocator
{
    public const string ConfigFileName = ".spriggit";
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;

    public SpriggitMetaLocator(
        ILogger logger,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }

    private FilePath? GetSpriggitConfigFile(DirectoryPath? outputFolder)
    {
        while (_fileSystem.Directory.Exists(outputFolder))
        {
            FilePath file = Path.Combine(outputFolder, ConfigFileName);
            if (file.Exists)
            {
                _logger.Information("Found .spriggit config at {ConfigPath}", file);
                return file;
            }

            outputFolder = outputFolder.Value.Directory;
        }

        return null;
    }
    
    public SpriggitMeta? Locate(DirectoryPath outputFolder)
    {
        var config = GetSpriggitConfigFile(outputFolder);
        if (config == null) return null;
        try
        {
            var meta = JsonConvert.DeserializeObject<SpriggitMetaSerialize>(_fileSystem.File.ReadAllText(config.Value));
            if (meta == null || meta.PackageName.IsNullOrWhitespace() || meta.Release == null)
            {
                return null;
            }

            _logger.Information("Loaded .spriggit config with {Meta}", meta);
            return new SpriggitMeta(
                new SpriggitSource()
                {
                    PackageName = meta.PackageName,
                    Version = meta.Version ?? "",
                },
                meta.Release.Value);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error reading spriggit config: {ConfigPath}", config.Value);
            Console.WriteLine(e);
            throw;
        }
    }
}