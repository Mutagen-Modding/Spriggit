using System.IO.Abstractions;
using AutoFixture.Xunit2;
using FluentAssertions;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.IO.DI;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using NSubstitute;
using Spriggit.Engine;
using Xunit;

namespace Spriggit.Tests;

public class PluginBackupCreatorTests
{
    public class Bootstrap
    {
        public PluginBackupCreator Sut { get; }

        public Bootstrap(
            IFileSystem fileSystem,
            IProvideCurrentTime provideCurrentTime)
        {
            this.Sut = new PluginBackupCreator(
                provideCurrentTime,
                fileSystem,
                new ModFilesMover(
                    fileSystem,
                    new AssociatedFilesLocator(
                        fileSystem)));
        }
    }
    
    [Theory, MutagenAutoData]
    public void Typical(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        ModPath existingModFile,
        string modContents,
        Bootstrap sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var backupPath = sut.Sut.Backup(existingModFile, 1);
        backupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(Path.Combine(backupPath!, existingModFile.ModKey.FileName)).Should().Be(modContents);
    }
    
    [Theory, MutagenModAutoData]
    public void StringsFiles(
        [Frozen] IProvideCurrentTime currentTime,
        SkyrimMod mod,
        Npc npc,
        string name,
        string french,
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        Bootstrap sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        mod.UsingLocalization = true;
        npc.Name ??= new(Language.English, name);
        npc.Name.Set(Language.French, french);
        ModPath modFile = Path.Combine(existingDir, mod.ModKey.FileName);
        fileSystem.Directory.CreateDirectory(modFile.Path.Directory!);
        mod.WriteToBinaryParallel(modFile, fileSystem: fileSystem);
        
        var backupPath = sut.Sut.Backup(modFile, 1);
        backupPath.Should().NotBeNull();
        fileSystem.File.Exists(Path.Combine(backupPath!, modFile.ModKey.FileName)).Should().BeTrue();
        var stringsFolder = Path.Combine(backupPath!, "Strings");
        fileSystem.Directory.Exists(stringsFolder).Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_English.STRINGS")).Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_English.DLSTRINGS")).Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_English.ILSTRINGS")).Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_French.STRINGS")).Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_French.DLSTRINGS")).Should().BeTrue();
        fileSystem.File.Exists(Path.Combine(stringsFolder, $"{mod.ModKey.Name}_French.ILSTRINGS")).Should().BeTrue();
    }
    
    [Theory, MutagenAutoData]
    public void NoBackupDesired(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        ModPath existingModFile,
        string modContents,
        Bootstrap sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var backupPath = sut.Sut.Backup(existingModFile, 0);
        backupPath.Should().BeNull();
    }
    
    [Theory, MutagenAutoData]
    public void NoFileNoBackup(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        ModPath modFile,
        Bootstrap sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        var backupPath = sut.Sut.Backup(modFile, 25);
        backupPath.Should().BeNull();
    }
    
    [Theory, MutagenAutoData]
    public void DoesNotSaveUnchangedContent(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        ModPath existingModFile,
        string modContents,
        Bootstrap sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var backupPath = sut.Sut.Backup(existingModFile, 1);
        backupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(Path.Combine(backupPath!, existingModFile.ModKey.FileName)).Should().Be(modContents);
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 15));
        var secondBackupPath = sut.Sut.Backup(existingModFile, 1);
        backupPath.Should().Be(secondBackupPath);
    }

    [Theory, MutagenAutoData]
    public void OtherBackupRemains(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        ModPath existingModFile,
        string modContents,
        ModPath existingOtherModFile,
        string otherModContents,
        Bootstrap sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        fileSystem.File.WriteAllText(existingOtherModFile, otherModContents);
        var otherBackupPath = sut.Sut.Backup(existingOtherModFile, 1);
        var backupPath = sut.Sut.Backup(existingModFile, 1);
        backupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(Path.Combine(backupPath!, existingModFile.ModKey.FileName)).Should().Be(modContents);
        otherBackupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(Path.Combine(otherBackupPath!, existingOtherModFile.ModKey.FileName)).Should().Be(otherModContents);
    }
    
    [Theory, MutagenAutoData]
    public async Task ExistingBackupRemains(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        ModPath existingModFile,
        string modContents,
        string modContents2,
        Bootstrap sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var backupPath = sut.Sut.Backup(existingModFile, 1);
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 14));
        fileSystem.File.WriteAllText(existingModFile, modContents2);
        var backupPath2 = sut.Sut.Backup(existingModFile, 2);
        backupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(Path.Combine(backupPath!, existingModFile.ModKey.FileName)).Should().Be(modContents);
        backupPath2.Should().NotBeNull();
        fileSystem.File.ReadAllText(Path.Combine(backupPath2!, existingModFile.ModKey.FileName)).Should().Be(modContents2);
    }
    
    [Theory, MutagenAutoData]
    public void CleanBackup(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        ModPath existingModFile,
        string modContents,
        string modContents2,
        Bootstrap sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var oldBackupPath = sut.Sut.Backup(existingModFile, 4);
        oldBackupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(Path.Combine(oldBackupPath!, existingModFile.ModKey.FileName)).Should().Be(modContents);
        fileSystem.File.WriteAllText(existingModFile, modContents);
        currentTime.Now.Returns(new DateTime(2024, 5, 25));
        var backupPath = sut.Sut.Backup(existingModFile, 4);
        fileSystem.File.Exists(Path.Combine(oldBackupPath!, existingModFile.ModKey.FileName)).Should().BeFalse();
        fileSystem.File.ReadAllText(Path.Combine(backupPath!, existingModFile.ModKey.FileName)).Should().Be(modContents);
    }
    
    [Theory, MutagenAutoData]
    public void CleanupOnlyAffectsTargetFile(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        ModPath existingModFile,
        ModPath otherExistingModFile,
        string modContents,
        string otherModContents,
        Bootstrap sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20));
        fileSystem.File.WriteAllText(otherExistingModFile, otherModContents);
        var otherOldBackupPath = sut.Sut.Backup(otherExistingModFile, 4);
        otherOldBackupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(Path.Combine(otherOldBackupPath!, otherExistingModFile.ModKey.FileName)).Should().Be(otherModContents);
        fileSystem.File.WriteAllText(existingModFile, modContents);
        currentTime.Now.Returns(new DateTime(2024, 5, 25));
        var backupPath = sut.Sut.Backup(existingModFile, 4);
        fileSystem.File.ReadAllText(Path.Combine(backupPath!, existingModFile.ModKey.FileName)).Should().Be(modContents);
        fileSystem.File.ReadAllText(Path.Combine(otherOldBackupPath!, otherExistingModFile.ModKey.FileName)).Should().Be(otherModContents);
    }
}