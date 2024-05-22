using System.IO.Abstractions;
using AutoFixture.Xunit2;
using FluentAssertions;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using NSubstitute;
using Spriggit.Engine;
using Xunit;

namespace Spriggit.Tests;

public class PluginBackupCreatorTests
{
    [Theory, MutagenAutoData]
    public void Typical(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        FilePath existingModFile,
        string modContents,
        PluginBackupCreator sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var backupPath = sut.Backup(existingModFile, 1);
        backupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(backupPath!).Should().Be(modContents);
    }
    
    [Theory, MutagenAutoData]
    public void NoBackupDesired(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        FilePath existingModFile,
        string modContents,
        PluginBackupCreator sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var backupPath = sut.Backup(existingModFile, 0);
        backupPath.Should().BeNull();
    }
    
    [Theory, MutagenAutoData]
    public void NoFileNoBackup(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        FilePath modFile,
        PluginBackupCreator sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        var backupPath = sut.Backup(modFile, 25);
        backupPath.Should().BeNull();
    }
    
    [Theory, MutagenAutoData]
    public void DoesNotSaveUnchangedContent(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        FilePath existingModFile,
        string modContents,
        PluginBackupCreator sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var backupPath = sut.Backup(existingModFile, 1);
        backupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(backupPath!).Should().Be(modContents);
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 15));
        var secondBackupPath = sut.Backup(existingModFile, 1);
        backupPath.Should().Be(secondBackupPath);
    }

    [Theory, MutagenAutoData]
    public void OtherBackupRemains(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        FilePath existingModFile,
        string modContents,
        FilePath existingOtherModFile,
        string otherModContents,
        PluginBackupCreator sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        fileSystem.File.WriteAllText(existingOtherModFile, otherModContents);
        var otherBackupPath = sut.Backup(existingOtherModFile, 1);
        var backupPath = sut.Backup(existingModFile, 1);
        backupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(backupPath!).Should().Be(modContents);
        otherBackupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(otherBackupPath!).Should().Be(otherModContents);
    }
    
    [Theory, MutagenAutoData]
    public async Task ExistingBackupRemains(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        FilePath existingModFile,
        string modContents,
        string modContents2,
        PluginBackupCreator sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 12));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var backupPath = sut.Backup(existingModFile, 1);
        currentTime.Now.Returns(new DateTime(2024, 5, 20, 6, 7, 14));
        fileSystem.File.WriteAllText(existingModFile, modContents2);
        var backupPath2 = sut.Backup(existingModFile, 2);
        backupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(backupPath!).Should().Be(modContents);
        backupPath2.Should().NotBeNull();
        fileSystem.File.ReadAllText(backupPath2!).Should().Be(modContents2);
    }
    
    [Theory, MutagenAutoData]
    public void CleanBackup(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        FilePath existingModFile,
        string modContents,
        string modContents2,
        PluginBackupCreator sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20));
        fileSystem.File.WriteAllText(existingModFile, modContents);
        var oldBackupPath = sut.Backup(existingModFile, 4);
        oldBackupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(oldBackupPath!).Should().Be(modContents);
        fileSystem.File.WriteAllText(existingModFile, modContents);
        currentTime.Now.Returns(new DateTime(2024, 5, 25));
        var backupPath = sut.Backup(existingModFile, 4);
        fileSystem.File.Exists(oldBackupPath!).Should().BeFalse();
        fileSystem.File.ReadAllText(backupPath!).Should().Be(modContents);
    }
    
    [Theory, MutagenAutoData]
    public void CleanupOnlyAffectsTargetFile(
        [Frozen] IProvideCurrentTime currentTime,
        IFileSystem fileSystem,
        FilePath existingModFile,
        FilePath otherExistingModFile,
        string modContents,
        string otherModContents,
        PluginBackupCreator sut)
    {
        currentTime.Now.Returns(new DateTime(2024, 5, 20));
        fileSystem.File.WriteAllText(otherExistingModFile, otherModContents);
        var otherOldBackupPath = sut.Backup(otherExistingModFile, 4);
        otherOldBackupPath.Should().NotBeNull();
        fileSystem.File.ReadAllText(otherOldBackupPath!).Should().Be(otherModContents);
        fileSystem.File.WriteAllText(existingModFile, modContents);
        currentTime.Now.Returns(new DateTime(2024, 5, 25));
        var backupPath = sut.Backup(existingModFile, 4);
        fileSystem.File.ReadAllText(backupPath!).Should().Be(modContents);
        fileSystem.File.ReadAllText(otherOldBackupPath!).Should().Be(otherModContents);
    }
}