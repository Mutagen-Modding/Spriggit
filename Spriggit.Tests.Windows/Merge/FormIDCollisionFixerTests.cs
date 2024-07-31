using System.IO.Abstractions;
using FluentAssertions;
using LibGit2Sharp;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Binary.Parameters;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.IO;
using Noggog.Testing.AutoFixture;
using Spriggit.Core;
using Spriggit.Engine.Merge;
using Spriggit.Yaml.Skyrim;
using Xunit;

namespace Spriggit.Tests.Windows.Merge;

public class FormIDCollisionFixerTests
{
    [Theory, MutagenModAutoData]
    public async Task NothingToFix(
        IFileSystem fileSystem,
        SkyrimMod mod,
        Npc n1,
        ModKey modKey,
        EntryPoint entryPoint,
        DirectoryPath modFolder,
        DirectoryPath modFolder2,
        DirectoryPath gitRootPath,
        DirectoryPath spriggitModPath,
        FormIDCollisionFixer sut)
    {
        var spriggitSource = new SpriggitSource()
        {
            PackageName = "Spriggit.Yaml.Skyrim",
            Version = "Test"
        };
        
        var modPath = Path.Combine(modFolder, mod.ModKey.FileName);
        fileSystem.Directory.CreateDirectory(modFolder);
        mod.WriteToBinary(modPath, new BinaryWriteParameters()
        {
            FileSystem = fileSystem
        });
        await entryPoint.Serialize(
            modPath, spriggitModPath, null,
            GameRelease.SkyrimSE,
            null, fileSystem, null,
            spriggitSource,
            CancellationToken.None);
        
        await sut.DetectAndFixInternal<ISkyrimMod, ISkyrimModGetter>(
            entryPoint,
            spriggitModPath: spriggitModPath,
            dataPath: null,
            meta: new SpriggitEmbeddedMeta(spriggitSource, GameRelease.SkyrimSE, modKey));

        var modPath2 = Path.Combine(modFolder2, mod.ModKey.FileName);
        fileSystem.Directory.CreateDirectory(modFolder2);
        await entryPoint.Deserialize(
            inputPath: spriggitModPath,
            outputPath: modPath2,
            dataPath: null,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None);

        var reimport = SkyrimMod.CreateFromBinary(modPath2, SkyrimRelease.SkyrimSE, new BinaryReadParameters()
        {
            FileSystem = fileSystem
        });
        reimport.EnumerateMajorRecords().Should().HaveCount(1);
        reimport.Npcs.First().FormKey.Should().Be(n1.FormKey);
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE, TargetFileSystem.Real)]
    public async Task SomethingToFix(
        SkyrimMod mod,
        Armor armor,
        EntryPoint entryPoint,
        FormIDCollisionFixer sut)
    {
        var spriggitSource = new SpriggitSource()
        {
            PackageName = "Spriggit.Yaml.Skyrim",
            Version = "Test"
        };
        using var tmp = TempFolder.FactoryByAddedPath(Path.Combine("SpriggitUnitTests", "FormIdCollisionFixer"), throwIfUnsuccessfulDisposal: false, deleteAfter: false);
        tmp.Dir.DeleteEntireFolder();
        
        var modFolder = Path.Combine(tmp.Dir, "ModFolder");
        Directory.CreateDirectory(modFolder);
        var modPath = Path.Combine(modFolder, mod.ModKey.FileName);
        var repoPath = Path.Combine(tmp.Dir, "Repo");
        Repository.Init(repoPath);
        var signature = new Signature("me", "someone@gmail.com", DateTimeOffset.Now);
        using var repo = new Repository(repoPath);
        var spriggitModPath = Path.Combine(repoPath, "Spriggit");
        
        // Make initial content
        File.WriteAllText(Path.Combine(tmp.Dir, "Readme.md"), "Readme");
        mod.WriteToBinary(modPath);
        await entryPoint.Serialize(
            modPath, spriggitModPath, null,
            GameRelease.SkyrimSE,
            null, null, null,
            spriggitSource,
            CancellationToken.None);

        Commands.Stage(repo, "*");
        repo.Commit("Initial", signature, signature, new CommitOptions());
        
        // Make branches
        var lhs = repo.CreateBranch("lhs");
        var rhs = repo.CreateBranch("rhs");

        var formKeyToCollide = mod.GetNextFormKey();

        // Setup lhs
        {
            Commands.Checkout(repo, lhs);
            var weap = new Weapon(formKeyToCollide, SkyrimRelease.SkyrimSE);
            mod.Weapons.Add(weap);
            mod.WriteToBinary(modPath);
            
            await entryPoint.Serialize(
                modPath, spriggitModPath,
                dataPath: null,
                GameRelease.SkyrimSE,
                null, null, null,
                spriggitSource,
                CancellationToken.None);
            
            Commands.Stage(repo, "*");
            repo.Commit("Weapon", signature, signature, new CommitOptions());
            mod.Weapons.Clear();
        }

        // Setup rhs
        {
            Commands.Checkout(repo, rhs);
            var npc = new Npc(formKeyToCollide, SkyrimRelease.SkyrimSE);
            mod.Npcs.Add(npc);
            mod.WriteToBinary(modPath);
            
            await entryPoint.Serialize(
                modPath, spriggitModPath,
                dataPath: null, GameRelease.SkyrimSE,
                null, null, null,
                spriggitSource,
                CancellationToken.None);
            
            Commands.Stage(repo, "*");
            repo.Commit("Npc", signature, signature, new CommitOptions());
            mod.Npcs.Clear();
        }
        
        // Merge
        repo.Merge(lhs, signature, new MergeOptions());

        await sut.DetectAndFixInternal<ISkyrimMod, ISkyrimModGetter>(
            entryPoint,
            spriggitModPath: spriggitModPath,
            dataPath: null,
            meta: new SpriggitEmbeddedMeta(spriggitSource, GameRelease.SkyrimSE, mod.ModKey));
        
        var modFolder2 = Path.Combine(tmp.Dir, "ModFolder2");
        var modPath2 = Path.Combine(modFolder2, mod.ModKey.FileName);
        Directory.CreateDirectory(modFolder2);
        await entryPoint.Deserialize(
            inputPath: spriggitModPath,
            outputPath: modPath2,
            dataPath: null,
            fileSystem: null,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None);
        
        var reimport = SkyrimMod.CreateFromBinary(modPath2, SkyrimRelease.SkyrimSE);
        reimport.EnumerateMajorRecords().Should().HaveCount(3);
        reimport.Armors.Select(x => x.FormKey)
            .Should().Equal(armor.FormKey);
        reimport.Npcs.Count.Should().Be(1);
        reimport.Weapons.Count.Should().Be(1);
        reimport.Npcs.First().FormKey.Should()
            .NotBe(reimport.Weapons.First().FormKey);
    }
}