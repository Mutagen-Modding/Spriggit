using System.IO.Abstractions;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.Core;
using Spriggit.Yaml.Starfield;
using Xunit;

namespace Spriggit.Tests.SpotTests;

public class StringsTests
{
    [Theory(Skip = "Translated string needs update in Mutagen"), MutagenModAutoData(GameRelease.Starfield)]
    public async Task DifferentModKeyStrings(
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
        mod.WriteToBinaryParallel(modPath, fileSystem: fileSystem);
        await entryPoint.Serialize(modPath, spriggitFolder, GameRelease.Starfield, workDropoff: null, fileSystem: fileSystem,
            streamCreator: null, new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "Test"
            }, CancellationToken.None);
        var modPath2 = Path.Combine(dataFolder2, otherModKey.ToString());
        await entryPoint.Deserialize(spriggitFolder, modPath2, workDropoff: null, fileSystem: fileSystem,
            streamCreator: null, CancellationToken.None);
        var otherStringsFolder = Path.Combine(dataFolder2, "Strings");
        var path = Path.Combine(otherStringsFolder, $"{otherModKey.Name}_en.STRINGS");
        fileSystem.File.Exists(path)
            .Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_fr.STRINGS"))
            .Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_en.ILSTRINGS"))
            .Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_fr.ILSTRINGS"))
            .Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_en.DLSTRINGS"))
            .Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{otherModKey.Name}_fr.DLSTRINGS"))
            .Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_en.STRINGS"))
            .Should().BeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_fr.STRINGS"))
            .Should().BeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_en.ILSTRINGS"))
            .Should().BeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_fr.ILSTRINGS"))
            .Should().BeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_en.DLSTRINGS"))
            .Should().BeFalse();
        fileSystem.File.Exists(Path.Combine(otherStringsFolder, $"{mod.ModKey.Name}_fr.DLSTRINGS"))
            .Should().BeFalse();
    }
}