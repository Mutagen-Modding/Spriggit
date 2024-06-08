using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Spriggit.Engine.Merge;
using Xunit;

namespace Spriggit.Tests.Merge;

public class FormIDReassignerTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public void NothingToReassign(
        StarfieldMod mod,
        Npc n1,
        Npc n2,
        FormIDReassigner sut)
    {
        sut.Reassign<IStarfieldMod, IStarfieldModGetter>(
            mod, 
            () => mod.GetNextFormKey(), 
            Array.Empty<IFormLinkIdentifier>());
        mod.EnumerateMajorRecords().Should().HaveCount(2);
        mod.Npcs.RecordCache.Count.Should().Be(2);
        mod.Npcs.RecordCache.ContainsKey(n1.FormKey).Should().BeTrue();
        mod.Npcs.RecordCache.ContainsKey(n2.FormKey).Should().BeTrue();
    }
    
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public void Reassign(
        StarfieldMod mod,
        Npc n1,
        FormIDReassigner sut)
    {
        var weap = new Weapon(n1.FormKey, StarfieldRelease.Starfield);
        mod.Weapons.Add(weap);

        FormKey replacementFormKeyOrig = mod.GetNextFormKey();
        FormKey? replacementFormKey = replacementFormKeyOrig;
        
        sut.Reassign<IStarfieldMod, IStarfieldModGetter>(
            mod,
            () =>
            {
                var toReturn = replacementFormKey;
                if (toReturn == null) throw new Exception();
                replacementFormKey = null;
                return toReturn.Value;
            }, 
            new IFormLinkIdentifier[]
            {
                weap
            });
        mod.EnumerateMajorRecords().Should().HaveCount(2);
        mod.Npcs.RecordCache.Count.Should().Be(1);
        mod.Npcs.RecordCache.ContainsKey(n1.FormKey).Should().BeTrue();
        mod.Weapons.RecordCache.Count.Should().Be(1);
        mod.Weapons.RecordCache.ContainsKey(weap.FormKey).Should().BeFalse();
        mod.Weapons.RecordCache.ContainsKey(replacementFormKeyOrig).Should().BeTrue();
    }
}