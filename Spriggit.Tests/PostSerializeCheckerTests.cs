using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Serilog;
using Spriggit.Core;
using Spriggit.Engine;
using Spriggit.Engine.Services.Singletons;
using Spriggit.Yaml.Starfield;
using Xunit;

namespace Spriggit.Tests;

public class PostSerializeCheckerTests
{
    public class Sut
    {
        public PostSerializeChecker Checker { get; }
        
        public Sut(IFileSystem fileSystem, ILogger logger)
        {
            Checker = new PostSerializeChecker(logger, fileSystem, null, null);
        }
    }
    
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task Typical(
        IFileSystem fileSystem,
        ILogger logger,
        DirectoryPath existingModFolder,
        DirectoryPath spriggitPath,
        EntryPoint entryPoint,
        StarfieldMod mod,
        Npc npc1,
        Weapon weapon1,
        Sut sut)
    {
        var modPath = Path.Combine(existingModFolder.Path, mod.ModKey.FileName);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });

        spriggitPath = Path.Combine(spriggitPath, mod.ModKey.FileName);
        
        await entryPoint.Serialize(
            modPath: modPath,
            outputDir: spriggitPath,
            dataPath: null,
            knownMasters: [],
            release: GameRelease.Starfield,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None,
            meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "1.2.3"
            });
        
        await sut.Checker.Check(
            mod: modPath,
            release: GameRelease.Starfield,
            spriggit: spriggitPath,
            dataPath: null,
            knownMasters: [],
            entryPt: new EngineEntryPointWrapper(
                logger,
                new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(1, 2, 3)),
                entryPoint),
            cancel: CancellationToken.None);
    }
    
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task SpriggitExtraMismatch(
        IFileSystem fileSystem,
        ILogger logger,
        DirectoryPath existingModFolder,
        DirectoryPath spriggitPath,
        EntryPoint entryPoint,
        StarfieldMod mod,
        Npc npc1,
        Weapon weapon1,
        Book book1,
        Sut sut)
    {
        var modPath = Path.Combine(existingModFolder.Path, mod.ModKey.FileName);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });

        spriggitPath = Path.Combine(spriggitPath, mod.ModKey.FileName);
        
        await entryPoint.Serialize(
            modPath: modPath,
            outputDir: spriggitPath,
            dataPath: null,
            knownMasters: [],
            release: GameRelease.Starfield,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None,
            meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "1.2.3"
            });
        
        mod.Books.Clear();
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });
        
        await Assert.ThrowsAsync<DataMisalignedException>(async () =>
        {
            await sut.Checker.Check(
                mod: modPath,
                release: GameRelease.Starfield,
                spriggit: spriggitPath,
                dataPath: null,
                knownMasters: [],
                entryPt: new EngineEntryPointWrapper(
                    logger,
                    new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(1, 2, 3)),
                    entryPoint),
                cancel: CancellationToken.None);
        });
    }
    
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task SpriggitMissingMismatch(
        IFileSystem fileSystem,
        ILogger logger,
        DirectoryPath existingModFolder,
        DirectoryPath spriggitPath,
        EntryPoint entryPoint,
        StarfieldMod mod,
        Npc npc1,
        Weapon weapon1,
        Sut sut)
    {
        var modPath = Path.Combine(existingModFolder.Path, mod.ModKey.FileName);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });

        spriggitPath = Path.Combine(spriggitPath, mod.ModKey.FileName);
        
        await entryPoint.Serialize(
            modPath: modPath,
            outputDir: spriggitPath,
            dataPath: null,
            knownMasters: [],
            release: GameRelease.Starfield,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None,
            meta: new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "1.2.3"
            });

        mod.Books.AddNew();
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });

        await Assert.ThrowsAsync<DataMisalignedException>(async () =>
        {
            await sut.Checker.Check(
                mod: modPath,
                release: GameRelease.Starfield,
                spriggit: spriggitPath,
                dataPath: null,
                knownMasters: [],
                entryPt: new EngineEntryPointWrapper(
                    logger,
                    new PackageIdentity("Spriggit.Yaml.Starfield", new NuGetVersion(1, 2, 3)),
                    entryPoint),
                cancel: CancellationToken.None);
        });
    }
}