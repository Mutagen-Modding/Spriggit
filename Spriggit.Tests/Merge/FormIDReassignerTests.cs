using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog.Testing.Extensions;
using Shouldly;
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
            []);
        mod.EnumerateMajorRecords().ShouldHaveCount(2);
        mod.Npcs.RecordCache.Count.ShouldBe(2);
        mod.Npcs.RecordCache.ContainsKey(n1.FormKey).ShouldBeTrue();
        mod.Npcs.RecordCache.ContainsKey(n2.FormKey).ShouldBeTrue();
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
        mod.EnumerateMajorRecords().ShouldHaveCount(2);
        mod.Npcs.RecordCache.Count.ShouldBe(1);
        mod.Npcs.RecordCache.ContainsKey(n1.FormKey).ShouldBeTrue();
        mod.Weapons.RecordCache.Count.ShouldBe(1);
        mod.Weapons.RecordCache.ContainsKey(weap.FormKey).ShouldBeFalse();
        mod.Weapons.RecordCache.ContainsKey(replacementFormKeyOrig).ShouldBeTrue();
    }
}