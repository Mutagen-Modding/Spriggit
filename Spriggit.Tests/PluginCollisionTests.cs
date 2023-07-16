using System.IO.Abstractions;
using Autofac;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.Core;
using Spriggit.Engine;
using Xunit;

namespace Spriggit.Tests;

public class PluginCollisionTests
{
    [Theory, MutagenAutoData]
    public async Task LoquiCollision(
        IFileSystem fileSystem,
        DirectoryPath existingFolder,
        DirectoryPath existingOutputDir,
        TestModule testModule)
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule(testModule);
        var cont = builder.Build();
        var c = cont.Resolve<EntryPointCache>();
        var jsonSrc = new SpriggitSource()
        {
            Version = "0.0.0.1-dev",
            PackageName = "Spriggit.Json.Skyrim"
        };
        var yamlSrc = new SpriggitSource()
        {
            Version = "0.0.0.1-dev",
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
  Version: 0.0.0.1-dev
ModKey: ModKey.esp
GameRelease: SkyrimSE";

        var modFolder = Path.Combine(existingFolder, "ModKey.esp");
        fileSystem.Directory.CreateDirectory(modFolder);
        var modFile = Path.Combine(modFolder, "RecordData.yaml");
        await fileSystem.File.WriteAllTextAsync(modFile, metaContent);

        await yamlEntry!.EntryPoint.Deserialize(
            modFolder,
            workDropoff: null,
            fileSystem: fileSystem,
            streamCreator: null,
            cancel: CancellationToken.None);
    }
}