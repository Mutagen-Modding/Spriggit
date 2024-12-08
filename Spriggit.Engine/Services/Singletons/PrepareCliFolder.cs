using System.IO.Abstractions;
using Noggog;
using Noggog.IO;
using NuGet.Versioning;

namespace Spriggit.Engine.Services.Singletons;

public class PrepareCliFolder
{
    public async Task Prepare(NuGetVersion version, CancellationToken cancel, DirectoryPath targetDir)
    {
        using var tmp = TempFolder.Factory();
        var path = Path.Combine(tmp.Dir, "SpriggitCLI.zip");
        var url = $"https://github.com/Mutagen-Modding/Spriggit/releases/download/{version.ToString().TrimStringFromEnd(".0")}/SpriggitCLI.zip";
        await DownloadFileAsync(
            url,
            Path.Combine(tmp.Dir, "SpriggitCLI.zip"),
            cancel);
        Directory.CreateDirectory(targetDir);
        System.IO.Compression.ZipFile.ExtractToDirectory(path, targetDir);
    }
    
    static async Task DownloadFileAsync(string url, string outputPath, CancellationToken cancel)
    {
        using (HttpClient client = new HttpClient())
        {
            var response = await client.GetAsync(url, cancel);
            response.EnsureSuccessStatusCode();

            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fs, cancel);
            }
        }
    }
}