using System.IO.Abstractions;
using DynamicData;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.Yaml.Starfield;
using Xunit;

namespace Spriggit.Tests.SpotTests;

public class DialogTopicInfoList : SpotTestBase
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
        var resps2 = topic.Responses.AddReturn(new DialogResponses(mod));
        var resp2 = resps2.Responses.AddReturn(new DialogResponse());
        topic.TopicInfoList ??= new();
        topic.TopicInfoList.Add(resps);
        resp.Edits = "Edits";
        resp2.Edits = "Edits2";

        var reimport = await TestUtil.PassThroughStarfield(fileSystem, mod, dataFolder, spriggitFolder, otherModKey, entryPoint);

        reimport.EnumerateMajorRecords().Should().HaveCount(4);
        reimport.Quests.Count.Should().Be(1);
        var reimportQuest = reimport.Quests.First();
        reimportQuest.DialogTopics.Count.Should().Be(1);
        var reimportTopic = reimportQuest.DialogTopics.First();
        reimportTopic.Responses.Count.Should().Be(2);
        var reimportResps = reimportTopic.Responses.First();
        reimportResps.Responses.Count.Should().Be(1);
        var reimportResp = reimportResps.Responses.First();
        reimportResp.Edits.Should().Be("Edits");
        var reimportResps2 = reimportTopic.Responses.Last();
        reimportResps.Responses.Count.Should().Be(1);
        var reimportResp2 = reimportResps2.Responses.First();
        reimportResp2.Edits.Should().Be("Edits2");
        reimportTopic.TopicInfoList.Should().NotBeNull();
        reimportTopic.TopicInfoList.Should().HaveCount(1);
        reimportTopic.TopicInfoList!.First().FormKey.ID.Should().Be(resps.FormKey.ID);
    }
}
