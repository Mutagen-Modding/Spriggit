using System.IO.Abstractions;
using AutoFixture.Xunit2;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Testing.AutoData;
using Newtonsoft.Json;
using Noggog;
using NSubstitute;
using Serilog;
using Shouldly;
using Spriggit.CLI.Lib;
using Spriggit.CLI.Lib.Commands;
using Spriggit.CLI.Lib.Commands.UpgradeTargetSpriggitVersionCommand;
using Spriggit.Core;
using Spriggit.Engine.Services.Singletons;
using Xunit;

namespace Spriggit.Tests;

public class UpgradeTargetSpriggitVersionRunnerTests
{

    [Theory, MutagenAutoData]
    public async Task Execute_WithNullMeta_ReturnsError(
        UpgradeTargetSpriggitVersionCommand command,
        [Frozen] ILogger logger,
        UpgradeTargetSpriggitVersionRunner sut)
    {
        // Act
        var result = await sut.Execute(command);

        // Assert
        // No meta file exists in the file system, so TryParseEmbeddedMeta will return null
        result.ShouldBe(1);
        logger.Received(1).Error(
            "Could not parse existing spriggit-meta.json in {SpriggitPath}",
            command.SpriggitPath);
    }
    
    [Theory, MutagenAutoData]
    public async Task Execute_WithSpecificPackageVersion_UpdatesMetaVersion(
        UpgradeTargetSpriggitVersionCommand command,
        DirectoryPath existingsSpriggitPath,
        ModKey modKey,
        IFileSystem fileSystem,
        SpriggitExternalMetaPersister persister,
        [Frozen] ILogger logger,
        [Frozen] ISpriggitEngine engine,
        UpgradeTargetSpriggitVersionRunner sut)
    {
        // Arrange
        command.PackageVersion = "0.40.0";
        command.SpriggitPath = existingsSpriggitPath;
        var originalMeta = new SpriggitModKeyMeta(
            new SpriggitSource { PackageName = "Spriggit.Yaml.Starfield", Version = "0.39.11" },
            GameRelease.Starfield,
            modKey);

        persister.Persist(existingsSpriggitPath, originalMeta);
    
        // Act
        var result = await sut.Execute(command);
    
        // Assert
        result.ShouldBe(0);
        logger.Received(1).Information("Updating spriggit-meta.json to version {Version}", command.PackageVersion);
    
        // Verify engine methods were called
        await engine.Received(1).Deserialize(
            command.SpriggitPath,
            Arg.Any<FilePath>(),
            command.DataFolder,
            0u,
            null,
            null,
            null,
            Arg.Any<CancellationToken?>());
    
        await engine.Received(1).Serialize(
            Arg.Any<ModPath>(),
            command.SpriggitPath,
            command.DataFolder,
            false,
            true,
            null,
            Arg.Is<SpriggitMeta>(m => m.Source.Version == command.PackageVersion),
            Arg.Any<CancellationToken?>());
    
        // Verify the meta file was actually updated
        var metaPath = Path.Combine(existingsSpriggitPath, "spriggit-meta.json");
        var updatedMetaJson = fileSystem.File.ReadAllText(metaPath);
        var updatedMeta = JsonConvert.DeserializeObject<SpriggitModKeyMetaSerialize>(updatedMetaJson, SpriggitExternalMetaPersister.JsonSettings);
        updatedMeta!.Version.ShouldBe(command.PackageVersion);
    }
    
    // [Theory, MutagenAutoData]
    // public async Task Execute_WithoutSpecificVersion_LogsCorrectMessage(
    //     UpgradeTargetSpriggitVersionCommand command,
    //     ModKey modKey,
    //     IFileSystem fileSystem)
    // {
    //     // Arrange
    //     command.PackageVersion = null; // No specific version provided
    //     var originalMeta = new SpriggitModKeyMeta(
    //         new SpriggitSource { PackageName = "Spriggit.Yaml.Starfield", Version = "0.39.11" },
    //         GameRelease.Starfield,
    //         modKey);
    //
    //     bootstrap.MetaPersister.TryParseEmbeddedMeta(command.SpriggitPath)
    //         .Returns(originalMeta);
    //
    //     // Act
    //     var result = await bootstrap.Sut.Execute(command);
    //
    //     // Assert
    //     result.ShouldBe(0);
    //     bootstrap.Logger.Received(1).Information("No specific version provided, will upgrade to latest during serialization");
    //
    //     // Verify that meta update was not called since no version was provided
    //     bootstrap.MetaUpdater.DidNotReceive().UpdateMetaVersion(Arg.Any<DirectoryPath>(), Arg.Any<string>());
    //
    //     // Verify serialization uses original version
    //     await bootstrap.Engine.Received(1).Serialize(
    //         Arg.Any<ModPath>(),
    //         command.SpriggitPath,
    //         command.DataFolder,
    //         false,
    //         true,
    //         null,
    //         Arg.Is<SpriggitMeta>(m => m.Source.Version == originalMeta.Source.Version),
    //         Arg.Any<CancellationToken?>());
    // }
    //
    // [Theory, MutagenAutoData]
    // public async Task Execute_WithEmptyPackageVersion_TreatsAsNull(
    //     UpgradeTargetSpriggitVersionCommand command,
    //     ModKey modKey,
    //     IFileSystem fileSystem)
    // {
    //     // Arrange
    //     command.PackageVersion = ""; // Empty string should be treated as null
    //     var originalMeta = new SpriggitModKeyMeta(
    //         new SpriggitSource { PackageName = "Spriggit.Yaml.Starfield", Version = "0.39.11" },
    //         GameRelease.Starfield,
    //         modKey);
    //
    //     bootstrap.MetaPersister.TryParseEmbeddedMeta(command.SpriggitPath)
    //         .Returns(originalMeta);
    //
    //     // Act
    //     var result = await bootstrap.Sut.Execute(command);
    //
    //     // Assert
    //     result.ShouldBe(0);
    //
    //     // Verify that meta update was not called since empty string is treated as null
    //     bootstrap.MetaUpdater.DidNotReceive().UpdateMetaVersion(Arg.Any<DirectoryPath>(), Arg.Any<string>());
    //     bootstrap.Logger.Received(1).Information("No specific version provided, will upgrade to latest during serialization");
    // }
    //
    // [Theory, MutagenAutoData]
    // public async Task Execute_PreservesOriginalModKeyInSerialization(
    //     UpgradeTargetSpriggitVersionCommand command,
    //     IFileSystem fileSystem)
    // {
    //     // Arrange
    //     var originalModKey = ModKey.FromNameAndExtension("Test Mod.esp");
    //     var originalMeta = new SpriggitModKeyMeta(
    //         new SpriggitSource { PackageName = "Spriggit.Yaml.Starfield", Version = "0.39.11" },
    //         GameRelease.Starfield,
    //         originalModKey);
    //
    //     bootstrap.MetaPersister.TryParseEmbeddedMeta(command.SpriggitPath)
    //         .Returns(originalMeta);
    //
    //     // Act
    //     var result = await bootstrap.Sut.Execute(command);
    //
    //     // Assert
    //     result.ShouldBe(0);
    //
    //     // Verify that the temp file is renamed to preserve the original ModKey
    //     await bootstrap.Engine.Received(1).Serialize(
    //         Arg.Is<ModPath>(path => Path.GetFileName(path) == originalModKey.FileName),
    //         command.SpriggitPath,
    //         command.DataFolder,
    //         false,
    //         true,
    //         null,
    //         Arg.Any<SpriggitMeta>(),
    //         Arg.Any<CancellationToken?>());
    // }
}