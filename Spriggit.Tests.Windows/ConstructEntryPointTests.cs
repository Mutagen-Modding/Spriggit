using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog.IO;
using Noggog.Testing.AutoFixture;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spriggit.Core;
using Spriggit.Engine.Services.Singletons;
using Xunit;
using Npc = Mutagen.Bethesda.Skyrim.Npc;

namespace Spriggit.Tests.Windows;

/// <summary>
/// These unit tests leave some trash in temp
/// </summary>
public class ConstructEntryPointTests
{
    private static TempFolder CreateDirFor([CallerMemberName] string? name = null)
    {
        var dir = Path.Combine("SpriggitUnitTests", nameof(ConstructEntryPointTests), name!);
        var temp = TempFolder.FactoryByAddedPath(
            dir,
            deleteAfter: false);
        temp.Dir.DeleteEntireFolder();
        temp.Dir.Create();
        return temp;
    }

    public class Payload
    {
        private readonly SkyrimMod _mod;
        private readonly Npc _npc;

        public Payload(
            DebugState debugState,
            PreparePluginFolder preparePluginFolder,
            ConstructEntryPoint sut,
            SkyrimMod mod,
            Npc npc)
        {
            _mod = mod;
            _npc = npc;
            PreparePluginFolder = preparePluginFolder;
            Sut = sut;
            debugState.ClearNugetSources = false;
        }

        public PreparePluginFolder PreparePluginFolder { get; }
        public ConstructEntryPoint Sut { get; }

        public async Task RunPassthrough(
            IEntryPoint entryPoint,
            PackageIdentity ident,
            [CallerMemberName] string? name = null)
        {
            var fs = new FileSystem();
            using var tmp = CreateDirFor(Path.Combine(name ?? "Unknown", "Passthrough"));
            var outputFolder = tmp.Dir;
            fs.Directory.CreateDirectory(outputFolder);
            var modPath = Path.Combine(outputFolder, _mod.ModKey.ToString());
            _mod.WriteToBinary(modPath, new BinaryWriteParameters()
            {
                FileSystem = fs
            });
            var serializeOutputFolder = Path.Combine(outputFolder, "serialize", _mod.ModKey.FileName);
            await entryPoint.Serialize(
                modPath,
                serializeOutputFolder,
                dataPath: null,
                knownMasters: [],
                GameRelease.SkyrimSE,
                null,
                fs,
                null,
                new SpriggitSource()
                {
                    Version = ident.Version.ToString(),
                    PackageName = ident.Id
                },
                CancellationToken.None);
            fs.Directory.CreateDirectory(Path.Combine(outputFolder, "deserialized"));
            var deserializeOutputFolder = Path.Combine(outputFolder, "deserialized", _mod.ModKey.FileName);
            await entryPoint.Deserialize(
                serializeOutputFolder,
                deserializeOutputFolder,
                dataPath: null,
                knownMasters: [],
                null,
                fs,
                null,
                CancellationToken.None);
            var reimport = SkyrimMod.CreateFromBinaryOverlay(
                modPath,
                SkyrimRelease.SkyrimSE,
                new BinaryReadParameters()
                {
                    FileSystem = fs
                });
            reimport.Npcs.Select(x => x.FormKey).Should().Equal(_npc.FormKey);
        }
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE, FileSystem: TargetFileSystem.Real)]
    public async Task TypicalExisting(Payload payload)
    {
        var ident = new PackageIdentity("Spriggit.Yaml.Skyrim", new NuGetVersion(0, 20, 0));
        using var tmp = CreateDirFor();
        var identFolder = Path.Combine(tmp.Dir, ident.ToString());
        await payload.PreparePluginFolder.Prepare(ident, CancellationToken.None, identFolder);
        using var entryPt = await payload.Sut.ConstructFor(tmp.Dir, ident, CancellationToken.None);
        entryPt.Should().NotBeNull();
        await payload.RunPassthrough(entryPt!, ident);
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE, FileSystem: TargetFileSystem.Real)]
    public async Task PackageDoesNotExist(Payload payload)
    {
        var ident = new PackageIdentity("Spriggit.Yaml.Skyrim", new NuGetVersion(255, 18, 0));
        using var tmp = CreateDirFor();
        using var entryPt = await payload.Sut.ConstructFor(tmp.Dir, ident, CancellationToken.None);
        entryPt.Should().BeNull();
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE, FileSystem: TargetFileSystem.Real)]
    public async Task PluginFolderMalformedShouldRetry(Payload payload)
    {
        var ident = new PackageIdentity("Spriggit.Yaml.Skyrim", new NuGetVersion(0, 20, 0));
        using var tmp = CreateDirFor();
        var identFolder = Path.Combine(tmp.Dir, ident.ToString());
        Directory.CreateDirectory(identFolder);
        using var entryPt = await payload.Sut.ConstructFor(tmp.Dir, ident, CancellationToken.None);
        entryPt.Should().NotBeNull();
        await payload.RunPassthrough(entryPt!, ident);
    }
}