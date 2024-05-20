using System.IO.Abstractions;
using FluentAssertions;
using LibGit2Sharp;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.IO;
using Noggog.Testing.AutoFixture;
using Spriggit.Core;
using Spriggit.Engine.Collision;
using Spriggit.Yaml.Starfield;
using Xunit;

namespace Spriggit.Tests.Collision;

public class FormIDCollisionFixerTests
{
#if OS_WINDOWS
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task NothingToFix(
        IFileSystem fileSystem,
        StarfieldMod mod,
        Npc n1,
        ModKey modKey,
        EntryPoint entryPoint,
        DirectoryPath modFolder,
        DirectoryPath modFolder2,
        DirectoryPath gitRootPath,
        DirectoryPath spriggitModPath,
        FormIDCollisionFixer sut)
    {
        var modPath = Path.Combine(modFolder, mod.ModKey.FileName);
        fileSystem.Directory.CreateDirectory(modFolder);
        mod.WriteToBinary(modPath, fileSystem: fileSystem);
        await entryPoint.Serialize(
            modPath, spriggitModPath, GameRelease.Starfield,
            null, fileSystem, null,
            new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "Test"
            },
            CancellationToken.None);
        
        await sut.DetectAndFixInternal<IStarfieldMod, IStarfieldModGetter>(
            entryPoint,
            spriggitModPath: spriggitModPath);

        var modPath2 = Path.Combine(modFolder2, mod.ModKey.FileName);
        fileSystem.Directory.CreateDirectory(modFolder2);
        await entryPoint.Deserialize(
            inputPath: spriggitModPath,
            outputPath: modPath2,
            fileSystem: fileSystem,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None);

        var reimport = StarfieldMod.CreateFromBinary(modPath2, StarfieldRelease.Starfield, fileSystem: fileSystem);
        reimport.EnumerateMajorRecords().Should().HaveCount(1);
        reimport.Npcs.First().FormKey.Should().Be(n1.FormKey);
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield, TargetFileSystem.Real)]
    public async Task SomethingToFix(
        StarfieldMod mod,
        Armor armor,
        EntryPoint entryPoint,
        FormIDCollisionFixer sut)
    {
        var spriggitSource = new SpriggitSource()
        {
            PackageName = "Spriggit.Yaml.Starfield",
            Version = "Test"
        };
        using var tmp = TempFolder.FactoryByAddedPath(Path.Combine("SpriggitUnitTests", "FormIdCollisionFixer"), throwIfUnsuccessfulDisposal: false);
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
            modPath, spriggitModPath, GameRelease.Starfield,
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
            var weap = new Weapon(formKeyToCollide, StarfieldRelease.Starfield);
            mod.Weapons.Add(weap);
            mod.WriteToBinary(modPath);
            
            await entryPoint.Serialize(
                modPath, spriggitModPath, GameRelease.Starfield,
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
            var npc = new Npc(formKeyToCollide, StarfieldRelease.Starfield);
            mod.Npcs.Add(npc);
            mod.WriteToBinary(modPath);
            
            await entryPoint.Serialize(
                modPath, spriggitModPath, GameRelease.Starfield,
                null, null, null,
                spriggitSource,
                CancellationToken.None);
            
            Commands.Stage(repo, "*");
            repo.Commit("Npc", signature, signature, new CommitOptions());
            mod.Npcs.Clear();
        }
        
        // Merge
        repo.Merge(lhs, signature, new MergeOptions());

        await sut.DetectAndFixInternal<IStarfieldMod, IStarfieldModGetter>(
            entryPoint,
            spriggitModPath: spriggitModPath);
        
        var modFolder2 = Path.Combine(tmp.Dir, "ModFolder2");
        var modPath2 = Path.Combine(modFolder2, mod.ModKey.FileName);
        Directory.CreateDirectory(modFolder2);
        await entryPoint.Deserialize(
            inputPath: spriggitModPath,
            outputPath: modPath2,
            fileSystem: null,
            workDropoff: null,
            streamCreator: null,
            cancel: CancellationToken.None);
        
        var reimport = StarfieldMod.CreateFromBinary(modPath2, StarfieldRelease.Starfield);
        reimport.EnumerateMajorRecords().Should().HaveCount(3);
        reimport.Armors.Select(x => x.FormKey)
            .Should().Equal(armor.FormKey);
        reimport.Npcs.Count.Should().Be(1);
        reimport.Weapons.Count.Should().Be(1);
        reimport.Npcs.First().FormKey.Should()
            .NotBe(reimport.Weapons.First().FormKey);
    }
#endif
}