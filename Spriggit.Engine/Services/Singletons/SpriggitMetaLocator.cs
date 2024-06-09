using System.IO.Abstractions;
using Newtonsoft.Json;
using Noggog;
using Serilog;
using Spriggit.Core;

namespace Spriggit.Engine.Services.Singletons;

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

    public FilePath? LocateSpriggitConfigFile(DirectoryPath? outputFolder)
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
    
    public SpriggitMeta? LocateAndParse(DirectoryPath outputFolder)
    {
        var config = LocateSpriggitConfigFile(outputFolder);
        return Parse(config);
    }
    
    public SpriggitMeta? Parse(FilePath? path)
    {
        if (path == null) return null;
        try
        {
            var meta = JsonConvert.DeserializeObject<SpriggitMetaSerialize>(_fileSystem.File.ReadAllText(path.Value));
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
            _logger.Error(e, "Error reading spriggit config: {ConfigPath}", path.Value);
            Console.WriteLine(e);
            throw;
        }
    }
}