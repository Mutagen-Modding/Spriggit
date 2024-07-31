namespace Spriggit.Core;

public interface ISimplisticEntryPoint
{
    public Task Serialize(
        string modPath,
        string outputDir,
        string? dataPath,
        int release,
        string packageName,
        string version,
        CancellationToken cancel);

    public Task Deserialize(
        string inputPath,
        string outputPath,
        string? dataPath,
        CancellationToken cancel);
    
    public Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(
        string inputPath,
        CancellationToken cancel);
}