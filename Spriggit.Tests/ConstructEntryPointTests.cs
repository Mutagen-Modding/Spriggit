using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.IO;
using Noggog.Testing.AutoFixture;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spriggit.Core;
using Spriggit.Engine;
using Spriggit.Engine.Services.Singletons;
using Xunit;

namespace Spriggit.Tests;

/// <summary>
/// These unit tests leave some trash in temp
/// </summary>
public class ConstructEntryPointTests
{
    private static TempFolder CreateDirFor([CallerMemberName] string? name = null)
    {
        var dir = Path.Combine("SpriggitUnitTests", name!);
        var temp = TempFolder.FactoryByAddedPath(
            dir,
            deleteAfter: false);
        temp.Dir.DeleteEntireFolder();
        temp.Dir.Create();
        return temp;
    }

    public class Payload
    {
        private readonly StarfieldMod _mod;
        private readonly Npc _npc;
        private readonly DirectoryPath _outputFolder;

        public Payload(
            DebugState debugState,
            PreparePluginFolder preparePluginFolder,
            ConstructEntryPoint sut,
            StarfieldMod mod,
            Npc npc,
            DirectoryPath outputFolder)
        {
            _mod = mod;
            _npc = npc;
            _outputFolder = outputFolder;
            PreparePluginFolder = preparePluginFolder;
            Sut = sut;
            debugState.ClearNugetSources = false;
        }

        public PreparePluginFolder PreparePluginFolder { get; }
        public ConstructEntryPoint Sut { get; }

        public async Task RunPassthrough(IEntryPoint entryPoint, PackageIdentity ident)
        {
            var fs = new MockFileSystem();
            fs.Directory.CreateDirectory(_outputFolder);
            var modPath = Path.Combine(_outputFolder, _mod.ModKey.ToString());
            _mod.WriteToBinary(modPath, new BinaryWriteParameters()
            {
                FileSystem = fs
            });
            var serializeOutputFolder = Path.Combine(_outputFolder, "serialize", _mod.ModKey.FileName);
            await entryPoint.Serialize(
                modPath,
                serializeOutputFolder,
                GameRelease.Starfield,
                null,
                fs,
                null,
                new SpriggitSource()
                {
                    Version = ident.Version.ToString(),
                    PackageName = ident.Id
                },
                CancellationToken.None);
            fs.Directory.CreateDirectory(Path.Combine(_outputFolder, "deserialized"));
            var deserializeOutputFolder = Path.Combine(_outputFolder, "deserialized", _mod.ModKey.FileName);
            await entryPoint.Deserialize(
                serializeOutputFolder,
                deserializeOutputFolder,
                null,
                fs,
                null,
                CancellationToken.None);
            var reimport = StarfieldMod.CreateFromBinaryOverlay(
                modPath,
                StarfieldRelease.Starfield,
                new BinaryReadParameters()
                {
                    FileSystem = fs
                });
            reimport.Npcs.Select(x => x.FormKey).Should().Equal(_npc.FormKey);
        }
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield, FileSystem: TargetFileSystem.Real)]
    public async Task TypicalExisting(Payload payload)
    {
        var ident = new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(0, 18, 0));
        using var tmp = CreateDirFor();
        var identFolder = Path.Combine(tmp.Dir, ident.ToString());
        await payload.PreparePluginFolder.Prepare(ident, CancellationToken.None, identFolder);
        using var entryPt = await payload.Sut.ConstructFor(tmp.Dir, ident, CancellationToken.None);
        entryPt.Should().NotBeNull();
        await payload.RunPassthrough(entryPt!, ident);
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield, FileSystem: TargetFileSystem.Real)]
    public async Task Version17(Payload payload)
    {
        var ident = new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(0, 17, 0));
        using var tmp = CreateDirFor();
        var identFolder = Path.Combine(tmp.Dir, ident.ToString());
        await payload.PreparePluginFolder.Prepare(ident, CancellationToken.None, identFolder);
        using var entryPt = await payload.Sut.ConstructFor(tmp.Dir, ident, CancellationToken.None);
        entryPt.Should().NotBeNull();
        await payload.RunPassthrough(entryPt!, ident);
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield, FileSystem: TargetFileSystem.Real)]
    public async Task Version16(Payload payload)
    {
        var ident = new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(0, 16, 0));
        using var tmp = CreateDirFor();
        var identFolder = Path.Combine(tmp.Dir, ident.ToString());
        await payload.PreparePluginFolder.Prepare(ident, CancellationToken.None, identFolder);
        using var entryPt = await payload.Sut.ConstructFor(tmp.Dir, ident, CancellationToken.None);
        entryPt.Should().NotBeNull();
        await payload.RunPassthrough(entryPt!, ident);
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield, FileSystem: TargetFileSystem.Real)]
    public async Task Version15(Payload payload)
    {
        var ident = new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(0, 15, 0));
        using var tmp = CreateDirFor();
        var identFolder = Path.Combine(tmp.Dir, ident.ToString());
        await payload.PreparePluginFolder.Prepare(ident, CancellationToken.None, identFolder);
        using var entryPt = await payload.Sut.ConstructFor(tmp.Dir, ident, CancellationToken.None);
        entryPt.Should().NotBeNull();
        await payload.RunPassthrough(entryPt!, ident);
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield, FileSystem: TargetFileSystem.Real)]
    public async Task PackageDoesNotExist(Payload payload)
    {
        var ident = new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(255, 18, 0));
        using var tmp = CreateDirFor();
        using var entryPt = await payload.Sut.ConstructFor(tmp.Dir, ident, CancellationToken.None);
        entryPt.Should().BeNull();
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield, FileSystem: TargetFileSystem.Real)]
    public async Task PluginFolderMalformedShouldRetry(Payload payload)
    {
        var ident = new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(0, 18, 0));
        using var tmp = CreateDirFor();
        var identFolder = Path.Combine(tmp.Dir, ident.ToString());
        Directory.CreateDirectory(identFolder);
        using var entryPt = await payload.Sut.ConstructFor(tmp.Dir, ident, CancellationToken.None);
        entryPt.Should().NotBeNull();
        await payload.RunPassthrough(entryPt!, ident);
    }
}