using Noggog;

namespace Spriggit.Engine;

public class SpriggitTempSourcesProvider
{
    public string SpriggitSourcesPath { get; } = new DirectoryPath(Path.Combine(Path.GetTempPath(), "Spriggit", "Sources"));
}