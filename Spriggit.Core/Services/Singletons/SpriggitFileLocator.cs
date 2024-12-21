using System.IO.Abstractions;
using Newtonsoft.Json;
using Noggog;
using Serilog;

namespace Spriggit.Core.Services.Singletons;

public class SpriggitFileLocator
{
    public const string ConfigFileName = ".spriggit";
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;

    public SpriggitFileLocator(
        ILogger logger,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }

    public FilePath? LocateSpriggitConfigFile(DirectoryPath? outputFolder)
    {
        while (outputFolder != null)
        {
            FilePath file = Path.Combine(outputFolder, ConfigFileName);
            if (file.CheckExists(_fileSystem))
            {
                _logger.Information($"Found {ConfigFileName} config at {{ConfigPath}}", file);
                return file;
            }

            outputFolder = outputFolder.Value.Directory;
        }

        return null;
    }
    
    public SpriggitFile? LocateAndParse(DirectoryPath outputFolder)
    {
        var config = LocateSpriggitConfigFile(outputFolder);
        return Parse(config);
    }
    
    public SpriggitFile? Parse(FilePath? path)
    {
        if (path == null) return null;
        try
        {
            var fileSerialize = JsonConvert.DeserializeObject<SpriggitFileSerialize>(_fileSystem.File.ReadAllText(path.Value));
            if (fileSerialize == null)
            {
                return null;
            }

            SpriggitMeta? source = null;
            if (!fileSerialize.PackageName.IsNullOrWhitespace()
                && fileSerialize.Release != null)
            {
                source = new SpriggitMeta(
                    new SpriggitSource()
                    {
                        PackageName = fileSerialize.PackageName,
                        Version = fileSerialize.Version ?? "",
                    },
                    fileSerialize.Release.Value);
            }

            _logger.Information($"Loaded {ConfigFileName} config with {{Meta}}", fileSerialize);
            return new SpriggitFile(
                source,
                fileSerialize.KnownMasters?
                    .Select(m => new KnownMaster(m.ModKey, m.Style))
                    .ToArray() ?? []);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error reading spriggit config: {ConfigPath}", path.Value);
            Console.WriteLine(e);
            throw;
        }
    }
}