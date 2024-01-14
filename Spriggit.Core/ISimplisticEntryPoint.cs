namespace Spriggit.Core;

public interface ISimplisticEntryPoint
{
    public Task Serialize(
        string modPath,
        string outputDir,
        int release,
        string packageName,
        string version,
        CancellationToken cancel);

    public Task Deserialize(
        string inputPath,
        string outputPath,
        CancellationToken cancel);
    
    public Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(
        string inputPath,
        CancellationToken cancel);
}