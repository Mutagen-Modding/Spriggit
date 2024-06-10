using System.IO.Abstractions;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.Engine.Services.Singletons;
using Xunit;
using SkyrimNpc = Mutagen.Bethesda.Skyrim.Npc;
using OblivionNpc = Mutagen.Bethesda.Oblivion.Npc;

namespace Spriggit.Tests;

public class LocalizeEnforcerTests
{
    [Theory, MutagenModAutoData(GameRelease.Oblivion)]
    public void NotAllowed(
        IFileSystem fileSystem,
        OblivionMod mod,
        OblivionNpc npc,
        DirectoryPath existingDir,
        LocalizeEnforcer sut)
    {
        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);
        mod.WriteToBinary(modPath, fileSystem: fileSystem);
        Assert.Throws<ArgumentException>(() =>
        {
            sut.Localize(localize: true, modPath, mod.GameRelease);
        });
    }
    
    [Theory, MutagenModAutoData(GameRelease.Oblivion)]
    public void NotAllowedButOff(
        IFileSystem fileSystem,
        OblivionMod mod,
        OblivionNpc npc,
        DirectoryPath existingDir,
        LocalizeEnforcer sut)
    {
        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);
        mod.WriteToBinary(modPath, fileSystem: fileSystem);
        sut.Localize(localize: false, modPath, mod.GameRelease);
    }
    
    [Theory, MutagenModAutoData]
    public void NothingToDoOff(
        IFileSystem fileSystem,
        SkyrimMod mod,
        SkyrimNpc npc,
        DirectoryPath existingDir,
        LocalizeEnforcer sut)
    {
        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);
        mod.UsingLocalization = false;
        mod.WriteToBinary(modPath, fileSystem: fileSystem);
        sut.Localize(localize: false, modPath, mod.GameRelease);
        using var reimport = SkyrimMod.CreateFromBinaryOverlay(modPath, mod.SkyrimRelease, fileSystem: fileSystem);
        reimport.UsingLocalization.Should().Be(false);
    }
    
    [Theory, MutagenModAutoData]
    public void NothingToDoOn(
        IFileSystem fileSystem,
        SkyrimMod mod,
        SkyrimNpc npc,
        DirectoryPath existingDir,
        LocalizeEnforcer sut)
    {
        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);
        mod.UsingLocalization = true;
        mod.WriteToBinary(modPath, fileSystem: fileSystem);
        sut.Localize(localize: true, modPath, mod.GameRelease);
        using var reimport = SkyrimMod.CreateFromBinaryOverlay(modPath, mod.SkyrimRelease, fileSystem: fileSystem);
        reimport.UsingLocalization.Should().Be(true);
    }
    
    [Theory, MutagenModAutoData]
    public void TypicalOn(
        IFileSystem fileSystem,
        SkyrimMod mod,
        SkyrimNpc npc,
        DirectoryPath existingDir,
        LocalizeEnforcer sut)
    {
        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);
        mod.UsingLocalization = false;
        mod.WriteToBinary(modPath, fileSystem: fileSystem);
        sut.Localize(localize: true, modPath, mod.GameRelease);
        using var reimport = SkyrimMod.CreateFromBinaryOverlay(modPath, mod.SkyrimRelease, fileSystem: fileSystem);
        reimport.UsingLocalization.Should().Be(true);
    }
    
    [Theory, MutagenModAutoData]
    public void TypicalOff(
        IFileSystem fileSystem,
        SkyrimMod mod,
        SkyrimNpc npc,
        DirectoryPath existingDir,
        LocalizeEnforcer sut)
    {
        var modPath = Path.Combine(existingDir, mod.ModKey.FileName);
        mod.UsingLocalization = true;
        mod.WriteToBinary(modPath, fileSystem: fileSystem);
        sut.Localize(localize: false, modPath, mod.GameRelease);
        using var reimport = SkyrimMod.CreateFromBinaryOverlay(modPath, mod.SkyrimRelease, fileSystem: fileSystem);
        reimport.UsingLocalization.Should().Be(false);
    }
}