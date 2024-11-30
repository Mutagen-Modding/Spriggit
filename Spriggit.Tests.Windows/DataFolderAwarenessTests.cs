using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Loqui;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Exceptions;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Serialization.Exceptions;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.IO;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Spriggit.Core;
using Spriggit.Engine.Services.Singletons;
using Xunit;

namespace Spriggit.Tests.Windows;

public class DataFolderAwarenessTests
{
    private static TempFolder CreateDirFor([CallerMemberName] string? name = null)
    {
        var dir = Path.Combine("SpriggitUnitTests", nameof(DataFolderAwarenessTests), name!);
        var temp = TempFolder.FactoryByAddedPath(
            dir,
            deleteAfter: false);
        temp.Dir.DeleteEntireFolder();
        temp.Dir.Create();
        return temp;
    }

    public class Payload
    {
        public Payload(
            ModKey normalMasterKey,
            ModKey smallMasterKey,
            ModKey mediumMasterKey,
            ModKey originatingKey,
            DebugState debugState,
            PreparePluginFolder preparePluginFolder,
            ConstructEntryPoint sut)
        {
            NormalMasterKey = normalMasterKey;
            SmallMasterKey = smallMasterKey;
            MediumMasterKey = mediumMasterKey;
            OriginatingKey = originatingKey;
            PreparePluginFolder = preparePluginFolder;
            Sut = sut;
            debugState.ClearNugetSources = false;
            FileSystem = new FileSystem();
        }

        public ModKey NormalMasterKey { get; }
        public ModKey SmallMasterKey { get; }
        public ModKey MediumMasterKey { get; }
        public ModKey OriginatingKey { get; }
        public PreparePluginFolder PreparePluginFolder { get; }
        public ConstructEntryPoint Sut { get; }
        public FileSystem FileSystem { get; }

        public async Task<string> RunPassthrough<TModGetter>(
            DirectoryPath outputFolder,
            DirectoryPath dataFolder,
            IEntryPoint entryPoint,
            PackageIdentity ident,
            TModGetter mod,
            bool useDataFolder)
            where TModGetter : IModGetter
        {
            FileSystem.Directory.CreateDirectory(outputFolder);
            var modPath = Path.Combine(dataFolder, mod.ModKey.ToString());
            var dataFolderToUse = useDataFolder ? dataFolder : default(DirectoryPath?);
            await mod.BeginWrite
                .ToPath(modPath)
                .WithLoadOrder(new ModKey[]
                {
                    NormalMasterKey,
                    MediumMasterKey,
                    SmallMasterKey
                })
                .WithDataFolder(dataFolder)
                .WithFileSystem(FileSystem)
                .WriteAsync();
            var serializeOutputFolder = Path.Combine(outputFolder, "serialize", mod.ModKey.FileName);
            await entryPoint.Serialize(
                modPath,
                serializeOutputFolder,
                dataPath: dataFolderToUse,
                knownMasters: [],
                mod.GameRelease,
                null,
                FileSystem,
                null,
                new SpriggitSource()
                {
                    Version = ident.Version.ToString(),
                    PackageName = ident.Id
                },
                CancellationToken.None);
            FileSystem.Directory.CreateDirectory(Path.Combine(outputFolder, "deserialized"));
            var deserializeOutputFolder = Path.Combine(outputFolder, "deserialized", mod.ModKey.FileName);
            await entryPoint.Deserialize(
                serializeOutputFolder,
                deserializeOutputFolder,
                dataPath: dataFolderToUse,
                knownMasters: [],
                null,
                FileSystem,
                null,
                CancellationToken.None);
            return deserializeOutputFolder;
        }
    }

    [Theory, MutagenAutoData]
    public async Task NonSeparated(
        Payload payload)
    {
        var normalMaster = new SkyrimMod(payload.NormalMasterKey, SkyrimRelease.SkyrimSE)
        {
            IsMaster = true,
        };
        var normalNpc = normalMaster.Npcs.AddNew();
        normalNpc.EditorID = "Normal";
        var smallMaster = new SkyrimMod(payload.SmallMasterKey, SkyrimRelease.SkyrimSE)
        {
            IsSmallMaster = true,
        };
        var smallNpc = smallMaster.Npcs.AddNew();
        smallNpc.EditorID = "Small";
        var originating = new SkyrimMod(payload.OriginatingKey, SkyrimRelease.SkyrimSE);
        var originatingNpc = originating.Npcs.AddNew();
        originatingNpc.EditorID = "Originating";

        originating.Npcs.GetOrAddAsOverride(normalNpc);
        originating.Npcs.GetOrAddAsOverride(smallNpc);

        using var tmp = CreateDirFor();
        var dataFolder = Path.Combine(tmp.Dir, "Data");
        payload.FileSystem.Directory.CreateDirectory(dataFolder);

        normalMaster.WriteToBinary(Path.Combine(dataFolder, normalMaster.ModKey.FileName));
        smallMaster.WriteToBinary(Path.Combine(dataFolder, smallMaster.ModKey.FileName));

        var ep = new Spriggit.Yaml.Skyrim.EntryPoint();
        var modPath = await payload.RunPassthrough(
            Path.Combine(tmp.Dir, "Spriggit"),
            dataFolder,
            ep,
            new PackageIdentity("Spriggit.Yaml.Skyrim", new NuGetVersion(0, 18, 0)),
            originating,
            useDataFolder: false);
        using var mod = SkyrimMod.Create(SkyrimRelease.SkyrimSE)
            .FromPath(modPath)
            .Construct();

        mod.Npcs.Should().HaveCount(3);
        mod.Npcs.TryGetValue(normalNpc.FormKey, out var normalNpcReimport)
            .Should().BeTrue();
        normalNpcReimport!.FormKey.Should().Be(normalNpc.FormKey);
        mod.Npcs.TryGetValue(smallNpc.FormKey, out var smallNpcReimport)
            .Should().BeTrue();
        smallNpcReimport!.FormKey.Should().Be(smallNpc.FormKey);
        mod.Npcs.TryGetValue(originatingNpc.FormKey, out var originatingNpcReimport)
            .Should().BeTrue();
        originatingNpcReimport!.FormKey.Should().Be(originatingNpc.FormKey);
    }

    [Theory, MutagenAutoData]
    public async Task NonSeparatedWithDataFolder(
        Payload payload)
    {
        var normalMaster = new SkyrimMod(payload.NormalMasterKey, SkyrimRelease.SkyrimSE)
        {
            IsMaster = true,
        };
        var normalNpc = normalMaster.Npcs.AddNew();
        normalNpc.EditorID = "Normal";
        var smallMaster = new SkyrimMod(payload.SmallMasterKey, SkyrimRelease.SkyrimSE)
        {
            IsSmallMaster = true,
        };
        var smallNpc = smallMaster.Npcs.AddNew();
        smallNpc.EditorID = "Small";
        var originating = new SkyrimMod(payload.OriginatingKey, SkyrimRelease.SkyrimSE);
        var originatingNpc = originating.Npcs.AddNew();
        originatingNpc.EditorID = "Originating";

        originating.Npcs.GetOrAddAsOverride(normalNpc);
        originating.Npcs.GetOrAddAsOverride(smallNpc);

        using var tmp = CreateDirFor();
        var dataFolder = Path.Combine(tmp.Dir, "Data");
        payload.FileSystem.Directory.CreateDirectory(dataFolder);

        normalMaster.WriteToBinary(Path.Combine(dataFolder, normalMaster.ModKey.FileName));
        smallMaster.WriteToBinary(Path.Combine(dataFolder, smallMaster.ModKey.FileName));

        var ep = new Spriggit.Yaml.Skyrim.EntryPoint();
        var modPath = await payload.RunPassthrough(
            Path.Combine(tmp.Dir, "Spriggit"),
            dataFolder,
            ep,
            new PackageIdentity("Spriggit.Yaml.Skyrim", new NuGetVersion(0, 18, 0)),
            originating,
            useDataFolder: true);
        using var mod = SkyrimMod.Create(SkyrimRelease.SkyrimSE)
            .FromPath(modPath)
            .Construct();

        mod.Npcs.Should().HaveCount(3);
        mod.Npcs.TryGetValue(normalNpc.FormKey, out var normalNpcReimport)
            .Should().BeTrue();
        normalNpcReimport!.FormKey.Should().Be(normalNpc.FormKey);
        mod.Npcs.TryGetValue(smallNpc.FormKey, out var smallNpcReimport)
            .Should().BeTrue();
        smallNpcReimport!.FormKey.Should().Be(smallNpc.FormKey);
        mod.Npcs.TryGetValue(originatingNpc.FormKey, out var originatingNpcReimport)
            .Should().BeTrue();
        originatingNpcReimport!.FormKey.Should().Be(originatingNpc.FormKey);
    }

    [Theory, MutagenAutoData]
    public async Task Separated(
        Payload payload)
    {
        var normalMaster = new StarfieldMod(payload.NormalMasterKey, StarfieldRelease.Starfield)
        {
            IsMaster = true,
        };
        var normalNpc = normalMaster.Npcs.AddNew();
        normalNpc.EditorID = "Normal";
        var smallMaster = new StarfieldMod(payload.SmallMasterKey, StarfieldRelease.Starfield)
        {
            IsSmallMaster = true,
        };
        var smallNpc = smallMaster.Npcs.AddNew();
        smallNpc.EditorID = "Small";
        var mediumMaster = new StarfieldMod(payload.MediumMasterKey, StarfieldRelease.Starfield)
        {
            IsMediumMaster = true,
        };
        var mediumNpc = mediumMaster.Npcs.AddNew();
        mediumNpc.EditorID = "Medium";
        var originating = new StarfieldMod(payload.OriginatingKey, StarfieldRelease.Starfield);
        var originatingNpc = originating.Npcs.AddNew();
        originatingNpc.EditorID = "Originating";

        originating.Npcs.GetOrAddAsOverride(normalNpc);
        originating.Npcs.GetOrAddAsOverride(smallNpc);
        originating.Npcs.GetOrAddAsOverride(mediumNpc);

        using var tmp = CreateDirFor();
        var dataFolder = Path.Combine(tmp.Dir, "Data");
        payload.FileSystem.Directory.CreateDirectory(dataFolder);

        normalMaster.WriteToBinary(Path.Combine(dataFolder, normalMaster.ModKey.FileName));
        smallMaster.WriteToBinary(Path.Combine(dataFolder, smallMaster.ModKey.FileName));
        mediumMaster.WriteToBinary(Path.Combine(dataFolder, mediumMaster.ModKey.FileName));

        var ep = new Spriggit.Yaml.Starfield.EntryPoint();
        await Assert.ThrowsAsync<MissingModMappingException>(async () =>
        {
            var modPath = await payload.RunPassthrough(
                Path.Combine(tmp.Dir, "Spriggit"),
                dataFolder,
                ep,
                new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(0, 18, 0)),
                originating,
                useDataFolder: false);
        });
    }

    [Theory, MutagenAutoData]
    public async Task SeparatedWithDataFolder(
        Payload payload)
    {
        var normalMaster = new StarfieldMod(payload.NormalMasterKey, StarfieldRelease.Starfield)
        {
            IsMaster = true,
        };
        var normalNpc = normalMaster.Npcs.AddNew();
        normalNpc.EditorID = "Normal";
        var smallMaster = new StarfieldMod(payload.SmallMasterKey, StarfieldRelease.Starfield)
        {
            IsSmallMaster = true,
        };
        var smallNpc = smallMaster.Npcs.AddNew();
        smallNpc.EditorID = "Small";
        var mediumMaster = new StarfieldMod(payload.MediumMasterKey, StarfieldRelease.Starfield)
        {
            IsMediumMaster = true,
        };
        var mediumNpc = mediumMaster.Npcs.AddNew();
        mediumNpc.EditorID = "Medium";
        var originating = new StarfieldMod(payload.OriginatingKey, StarfieldRelease.Starfield);
        var originatingNpc = originating.Npcs.AddNew();
        originatingNpc.EditorID = "Originating";

        originating.Npcs.GetOrAddAsOverride(normalNpc);
        originating.Npcs.GetOrAddAsOverride(smallNpc);
        originating.Npcs.GetOrAddAsOverride(mediumNpc);

        using var tmp = CreateDirFor();
        var dataFolder = Path.Combine(tmp.Dir, "Data");
        payload.FileSystem.Directory.CreateDirectory(dataFolder);

        normalMaster.WriteToBinary(Path.Combine(dataFolder, normalMaster.ModKey.FileName));
        smallMaster.WriteToBinary(Path.Combine(dataFolder, smallMaster.ModKey.FileName));
        mediumMaster.WriteToBinary(Path.Combine(dataFolder, mediumMaster.ModKey.FileName));

        var ep = new Spriggit.Yaml.Starfield.EntryPoint();
        var modPath = await payload.RunPassthrough(
            Path.Combine(tmp.Dir, "Spriggit"),
            dataFolder,
            ep,
            new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(0, 18, 0)),
            originating,
            useDataFolder: true);
        using var mod = StarfieldMod.Create(StarfieldRelease.Starfield)
            .FromPath(modPath)
            .WithLoadOrderFromHeaderMasters()
            .WithDataFolder(dataFolder)
            .Construct();

        mod.Npcs.Should().HaveCount(4);
        mod.Npcs.TryGetValue(normalNpc.FormKey, out var normalNpcReimport)
            .Should().BeTrue();
        normalNpcReimport!.FormKey.Should().Be(normalNpc.FormKey);
        mod.Npcs.TryGetValue(smallNpc.FormKey, out var smallNpcReimport)
            .Should().BeTrue();
        smallNpcReimport!.FormKey.Should().Be(smallNpc.FormKey);
        mod.Npcs.TryGetValue(mediumNpc.FormKey, out var mediumNpcReimport)
            .Should().BeTrue();
        mediumNpcReimport!.FormKey.Should().Be(mediumNpc.FormKey);
        mod.Npcs.TryGetValue(originatingNpc.FormKey, out var originatingNpcReimport)
            .Should().BeTrue();
        originatingNpcReimport!.FormKey.Should().Be(originatingNpc.FormKey);
    }
}