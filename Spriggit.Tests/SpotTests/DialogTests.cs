using System.IO.Abstractions;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.Yaml.Starfield;
using Xunit;

namespace Spriggit.Tests.SpotTests;

public class DialogTests : SpotTestBase
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task DialogReponses(
        IFileSystem fileSystem,
        StarfieldMod mod,
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        ModKey otherModKey,
        EntryPoint entryPoint)
    {
        var quest = mod.Quests.AddNew();
        var topic = quest.DialogTopics.AddReturn(new DialogTopic(mod));
        var resps = topic.Responses.AddReturn(new DialogResponses(mod));
        var resp = resps.Responses.AddReturn(new DialogResponse());
        resp.Edits = "Edits";

        var reimport = await TestStarfieldUtil.PassThrough(fileSystem, mod, dataFolder, spriggitFolder, otherModKey, entryPoint);

        reimport.EnumerateMajorRecords().Should().HaveCount(3);
        reimport.Quests.Count.Should().Be(1);
        var reimportQuest = reimport.Quests.First();
        reimportQuest.DialogTopics.Count.Should().Be(1);
        var reimportTopic = reimportQuest.DialogTopics.First();
        reimportTopic.Responses.Count.Should().Be(1);
        var reimportResps = reimportTopic.Responses.First();
        reimportResps.Responses.Count.Should().Be(1);
        var reimportResp = reimportResps.Responses.First();
        reimportResp.Edits.Should().Be("Edits");
    }
}