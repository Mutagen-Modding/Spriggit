using Noggog;

namespace Spriggit.Engine.Services.Singletons;

public class SpriggitTempSourcesProvider
{
    public string SpriggitSourcesPath { get; } = new DirectoryPath(Path.Combine(Path.GetTempPath(), "Spriggit", "Sources"));
}