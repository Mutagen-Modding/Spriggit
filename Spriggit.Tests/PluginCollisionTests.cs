using System.IO.Abstractions;
using Autofac;
using FluentAssertions;
using Mutagen.Bethesda;
using Noggog.IO;
using Spriggit.Core;
using Spriggit.Engine;
using Spriggit.Engine.Services.Singletons;
using Spriggit.Tests.Utility;
using Xunit;

namespace Spriggit.Tests;

public class PluginCollisionTests
{
    [Fact(Skip = "Used to reproduce a bug")]
    public async Task LoquiCollision()
    {
        var fileSystem = new FileSystem();
        var testModule = new TestModule(fileSystem);
        var builder = new ContainerBuilder();
        builder.RegisterModule(testModule);
        var cont = builder.Build();
        var c = cont.Resolve<EntryPointCache>();
        var jsonSrc = new SpriggitSource()
        {
            Version = "0.0.0.1-zdev",
            PackageName = "Spriggit.Json.Skyrim"
        };
        var yamlSrc = new SpriggitSource()
        {
            Version = "0.0.0.1-zdev",
            PackageName = "Spriggit.Yaml.Skyrim"
        };
        var jsonEntry = await c.GetFor(new SpriggitMeta(
                jsonSrc,
                GameRelease.SkyrimSE),
            CancellationToken.None);
        jsonEntry.Should().NotBeNull();
        var yamlEntry = await c.GetFor(new SpriggitMeta(
                yamlSrc,
                GameRelease.SkyrimSE),
            CancellationToken.None);
        yamlEntry.Should().NotBeNull();

        var metaContent = @"SpriggitSource:
  PackageName: Spriggit.Yaml.Skyrim
  Version: 0.0.0.1-zdev
ModKey: ModKey.esp
GameRelease: SkyrimSE";

        using var tempDir = TempFolder.Factory();
        var existingFolder = Path.Combine(tempDir.Dir, "SomeFolder");
        fileSystem.Directory.CreateDirectory(existingFolder);
        var existingOutputDir = Path.Combine(tempDir.Dir, "OutputFolder");
        fileSystem.Directory.CreateDirectory(existingOutputDir);

        var modFolder = Path.Combine(existingFolder, "ModKey.esp");
        fileSystem.Directory.CreateDirectory(modFolder);
        var modFile = Path.Combine(modFolder, "RecordData.yaml");
        await fileSystem.File.WriteAllTextAsync(modFile, metaContent);

        await yamlEntry!.Deserialize(
            inputPath: modFolder,
            outputPath: Path.Combine(existingOutputDir, "ModKey.esp"),
            dataPath: null,
            workDropoff: null,
            knownMasters: [],
            fileSystem: fileSystem,
            streamCreator: null,
            cancel: CancellationToken.None);
    }
}