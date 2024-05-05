using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Spriggit.Engine.Collision;
using Xunit;

namespace Spriggit.Tests.Collision;

public class FormIDCollisionDetectorTests
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public void NoCollision(
        StarfieldMod mod,
        Npc n1,
        Npc n2,
        FormIDCollisionDetector sut)
    {
        mod.Npcs.Count.Should().Be(2);
        sut.LocateCollisions(mod)
            .Should().BeEmpty();
    }
    
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public void Collision(
        StarfieldMod mod,
        Npc n1,
        FormIDCollisionDetector sut)
    {
        var weap = new Weapon(new FormKey(mod.ModKey, n1.FormKey.ID), StarfieldRelease.Starfield);
        mod.Weapons.Add(weap);
        sut.LocateCollisions(mod)
            .Keys
            .Should().Equal(n1.FormKey);
    }
}