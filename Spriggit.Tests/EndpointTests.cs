using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Core;
using Spriggit.Yaml.Starfield;
using Xunit;

namespace Spriggit.Tests;

[Collection("Sequential")]
public class EndpointTests
{
    [Theory]
    [MutagenModAutoData(GameRelease.Starfield)]
    public async Task Deserialize(
        IFileSystem fileSystem,
        StarfieldMod mod,
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        EntryPoint entryPoint)
    {
        var modPath = new ModPath(Path.Combine(dataFolder, mod.ModKey.ToString()));
        fileSystem.Directory.CreateDirectory(dataFolder);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });
        await entryPoint.Serialize(modPath: modPath,
            outputDir: spriggitFolder,
            dataPath: dataFolder,
            release: GameRelease.Starfield,
            workDropoff: null,
            knownMasters: [],
            fileSystem: fileSystem,
            streamCreator: null, meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "Test"
            },
            throwOnUnknown: true,
            cancel: CancellationToken.None);
        await entryPoint.Deserialize(inputPath: spriggitFolder,
            outputPath: modPath,
            dataPath: dataFolder,
            workDropoff: null,
            knownMasters: [],
            fileSystem: fileSystem,
            streamCreator: null, cancel: CancellationToken.None);
        fileSystem.File.Exists(modPath)
            .ShouldBeTrue();
        var stringsFolder = Path.Combine(dataFolder, "Strings");
        fileSystem.Directory.Exists(stringsFolder)
            .ShouldBeFalse();
    }
    
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task DeserializeLocalized(
        IFileSystem fileSystem,
        StarfieldMod mod,
        Armor armor,
        string name,
        string frenchName,
        DirectoryPath dataFolder,
        DirectoryPath dataFolder2,
        DirectoryPath spriggitFolder,
        EntryPoint entryPoint)
    {
        try
        {
            mod.Armors.Count.ShouldBe(1);
            mod.UsingLocalization = true;
            armor.Name = name;
            armor.Name.Set(Language.French, frenchName);
            fileSystem.Directory.CreateDirectory(dataFolder);
            fileSystem.Directory.CreateDirectory(dataFolder2);
            var modPath = new ModPath(Path.Combine(dataFolder, mod.ModKey.ToString()));
            mod.WriteToBinary(modPath, new BinaryWriteParameters()
            {
                FileSystem = fileSystem
            });
            await entryPoint.Serialize(modPath: modPath,
                outputDir: spriggitFolder,
                dataPath: dataFolder,
                release: GameRelease.Starfield,
                workDropoff: null,
                knownMasters: [],
                fileSystem: fileSystem,
                streamCreator: null, meta: new SpriggitSource()
                {
                    PackageName = "Spriggit.Yaml.Starfield",
                    Version = "Test"
                },
                throwOnUnknown: true,
                cancel: CancellationToken.None);
            var modPath2 = new ModPath(Path.Combine(dataFolder2, mod.ModKey.ToString()));
            await entryPoint.Deserialize(inputPath: spriggitFolder,
                outputPath: modPath2,
                dataPath: dataFolder2,
                workDropoff: null,
                knownMasters: [],
                fileSystem: fileSystem,
                streamCreator: null, cancel: CancellationToken.None);
            fileSystem.File.Exists(modPath2)
                .ShouldBeTrue();
            var stringsFolder = Path.Combine(dataFolder2, "Strings");
            fileSystem.Directory.Exists(stringsFolder)
                .ShouldBeTrue();
            fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_en.STRINGS"))
                .ShouldBeTrue();
            fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_fr.STRINGS"))
                .ShouldBeTrue();
            fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_en.ILSTRINGS"))
                .ShouldBeTrue();
            fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_fr.ILSTRINGS"))
                .ShouldBeTrue();
            fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_en.DLSTRINGS"))
                .ShouldBeTrue();
            fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_fr.DLSTRINGS"))
                .ShouldBeTrue();
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task DeserializeDifferentModKeyStrings(
        IFileSystem fileSystem,
        StarfieldMod mod,
        Armor armor,
        string name,
        string frenchName,
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        DirectoryPath dataFolder2,
        ModKey otherModKey,
        EntryPoint entryPoint)
    {
        mod.UsingLocalization = true;
        armor.Name = name;
        armor.Name.Set(Language.French, frenchName);
        var modPath = new ModPath(Path.Combine(dataFolder, mod.ModKey.ToString()));
        fileSystem.Directory.CreateDirectory(dataFolder);
        fileSystem.Directory.CreateDirectory(dataFolder2);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });
        await entryPoint.Serialize(modPath: modPath,
            outputDir: spriggitFolder,
            dataPath: dataFolder,
            release: GameRelease.Starfield,
            workDropoff: null,
            knownMasters: [],
            fileSystem: fileSystem,
            streamCreator: null, meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "Test"
            },
            throwOnUnknown: true,
            cancel: CancellationToken.None);
        var modPath2 = Path.Combine(dataFolder2, otherModKey.ToString());
        await entryPoint.Deserialize(inputPath: spriggitFolder,
            outputPath: modPath2,
            dataPath: dataFolder2,
            workDropoff: null,
            knownMasters: [],
            fileSystem: fileSystem,
            streamCreator: null, cancel: CancellationToken.None);
        var otherStringsFolder = Path.Combine(dataFolder2, "Strings");
        var path = Path.Combine(otherStringsFolder, $"{otherModKey.Name}_en.STRINGS");
        fileSystem.File.Exists(path)
            .ShouldBeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_fr.STRINGS"))
            .ShouldBeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_en.ILSTRINGS"))
            .ShouldBeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_fr.ILSTRINGS"))
            .ShouldBeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_en.DLSTRINGS"))
            .ShouldBeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_fr.DLSTRINGS"))
            .ShouldBeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_en.STRINGS"))
            .ShouldBeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_fr.STRINGS"))
            .ShouldBeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_en.ILSTRINGS"))
            .ShouldBeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_fr.ILSTRINGS"))
            .ShouldBeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_en.DLSTRINGS"))
            .ShouldBeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_fr.DLSTRINGS"))
            .ShouldBeFalse();
    }
}