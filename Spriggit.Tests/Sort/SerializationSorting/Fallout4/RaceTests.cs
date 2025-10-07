using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Core;
using Xunit;
using Fallout4EntryPoint = Spriggit.Yaml.Fallout4.EntryPoint;

namespace Spriggit.Tests.Sort.SerializationSorting.Fallout4;

/// <summary>
/// Tests that verify Race sorting functionality works correctly
/// through the full serialization -> deserialization cycle using Spriggit for Fallout4
/// </summary>
public class RaceTests
{
    [Theory, MutagenModAutoData(GameRelease.Fallout4)]
    public async Task Fallout4RaceHeadData_SortsCorrectlyThroughSerialization(
        IFileSystem fileSystem,
        DirectoryPath tempDir,
        Fallout4EntryPoint entryPoint,
        Fallout4Mod mod)
    {
        // Create a race with out-of-order head data morph groups
        var race = mod.Races.AddNew("TestRace");
        race.HeadData = new GenderedItem<HeadData?>(
            male: new Mutagen.Bethesda.Fallout4.HeadData()
            {
                MorphGroups = new Noggog.ExtendedList<MorphGroup>()
                {
                    new MorphGroup() { Name = "Xyz" },
                    new MorphGroup() { Name = "Abc" },
                    new MorphGroup() { Name = "Mno" }
                }
            },
            female: new Mutagen.Bethesda.Fallout4.HeadData()
            {
                MorphGroups = new Noggog.ExtendedList<MorphGroup>()
                {
                    new MorphGroup() { Name = "Def" },
                    new MorphGroup() { Name = "Ghi" },
                    new MorphGroup() { Name = "Bcd" }
                }
            });

        // Verify initial order is NOT sorted
        var initialMaleMorphGroupNames = race.HeadData.Male?.MorphGroups?.Select(mg => mg.Name).ToArray();
        var initialFemaleMorphGroupNames = race.HeadData.Female?.MorphGroups?.Select(mg => mg.Name).ToArray();

        initialMaleMorphGroupNames.ShouldBe(new[] { "Xyz", "Abc", "Mno" });
        initialFemaleMorphGroupNames.ShouldBe(new[] { "Def", "Ghi", "Bcd" });

        // Perform serialization cycle and get deserialized mod
        var deserializedMod = await SerializationTestHelper.SerializeAndDeserialize(mod, tempDir, entryPoint, fileSystem);

        var deserializedRace = deserializedMod.Races.First();
        var sortedMaleMorphGroupNames = deserializedRace.HeadData?.Male?.MorphGroups?.Select(mg => mg.Name).ToArray();
        var sortedFemaleMorphGroupNames = deserializedRace.HeadData?.Female?.MorphGroups?.Select(mg => mg.Name).ToArray();

        // Morph groups should now be sorted by Name
        sortedMaleMorphGroupNames.ShouldBe(new[] { "Abc", "Mno", "Xyz" });
        sortedFemaleMorphGroupNames.ShouldBe(new[] { "Bcd", "Def", "Ghi" });
    }
}