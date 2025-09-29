using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Core;
using Xunit;
using StarfieldEntryPoint = Spriggit.Yaml.Starfield.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Starfield;

/// <summary>
/// Tests that verify Race sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Starfield
/// </summary>
public class RaceTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task StarfieldRaceChargen_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        StarfieldEntryPoint entryPoint,
        StarfieldMod mod)
    {
        // Create a race with out-of-order chargen morph groups
        var race = mod.Races.AddNew("TestRace");

        // Initialize all required biped objects to avoid issues
        for (int key = 0; key < 64; ++key)
        {
            race.BipedObjects[(Mutagen.Bethesda.Starfield.BipedObject)key] = new();
        }

        race.ChargenAndSkintones = new GenderedItem<ChargenAndSkintones?>(
            male: new ChargenAndSkintones()
            {
                Chargen = new Chargen()
                {
                    MorphGroups = new Noggog.ExtendedList<Mutagen.Bethesda.Starfield.MorphGroup>()
                    {
                        new Mutagen.Bethesda.Starfield.MorphGroup() { Name = "Xyz" },
                        new Mutagen.Bethesda.Starfield.MorphGroup() { Name = "Abc" },
                        new Mutagen.Bethesda.Starfield.MorphGroup() { Name = "Mno" }
                    }
                }
            },
            female: new ChargenAndSkintones()
            {
                Chargen = new Chargen()
                {
                    MorphGroups = new Noggog.ExtendedList<Mutagen.Bethesda.Starfield.MorphGroup>()
                    {
                        new Mutagen.Bethesda.Starfield.MorphGroup() { Name = "Def" },
                        new Mutagen.Bethesda.Starfield.MorphGroup() { Name = "Ghi" },
                        new Mutagen.Bethesda.Starfield.MorphGroup() { Name = "Bcd" }
                    }
                }
            });

        // Verify initial order is NOT sorted
        var initialMaleMorphGroupNames = race.ChargenAndSkintones.Male?.Chargen?.MorphGroups?.Select(mg => mg.Name).ToArray();
        var initialFemaleMorphGroupNames = race.ChargenAndSkintones.Female?.Chargen?.MorphGroups?.Select(mg => mg.Name).ToArray();

        initialMaleMorphGroupNames.ShouldBe(new[] { "Xyz", "Abc", "Mno" });
        initialFemaleMorphGroupNames.ShouldBe(new[] { "Def", "Ghi", "Bcd" });

        // Paths for serialization cycle
        var modPath = Path.Combine(tempDir.Path, mod.ModKey.FileName);
        var spriggitPath = Path.Combine(tempDir.Path, "spriggit");
        var deserializedModPath = Path.Combine(tempDir.Path, "deserialized.esm");

        // Create directories and write original mod to disk
        fileSystem.Directory.CreateDirectory(tempDir);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });

        // Serialize with Spriggit
        await entryPoint.Serialize(
            modPath: modPath,
            outputDir: spriggitPath,
            dataPath: null,
            knownMasters: [],
            release: GameRelease.Starfield,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None,
            meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "1.0.0"
            },
            throwOnUnknown: false);

        // Deserialize back to mod file
        await entryPoint.Deserialize(
            inputPath: spriggitPath,
            outputPath: deserializedModPath,
            dataPath: null,
            knownMasters: [],
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None);

        // Read the deserialized mod and verify sorting
        var deserializedMod = StarfieldMod.CreateFromBinary(deserializedModPath, StarfieldRelease.Starfield, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });

        var deserializedRace = deserializedMod.Races.First();
        var sortedMaleMorphGroupNames = deserializedRace.ChargenAndSkintones?.Male?.Chargen?.MorphGroups?.Select(mg => mg.Name).ToArray();
        var sortedFemaleMorphGroupNames = deserializedRace.ChargenAndSkintones?.Female?.Chargen?.MorphGroups?.Select(mg => mg.Name).ToArray();

        // Morph groups should now be sorted by Name
        sortedMaleMorphGroupNames.ShouldBe(new[] { "Abc", "Mno", "Xyz" });
        sortedFemaleMorphGroupNames.ShouldBe(new[] { "Bcd", "Def", "Ghi" });
    }
}