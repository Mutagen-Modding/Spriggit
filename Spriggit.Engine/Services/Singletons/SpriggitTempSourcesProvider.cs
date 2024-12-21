using Noggog;

namespace Spriggit.Engine.Services.Singletons;

public class SpriggitTempSourcesProvider
{
    public string SpriggitTempPath { get; } = new DirectoryPath(
        Path.Combine(Path.GetTempPath(), "Spriggit"));

    public const string SourcesSubPath = "Sources";

    public const string ClisSubPath = "CLIs";

    public const string TranslationToolsSubPath = "Translations";
}