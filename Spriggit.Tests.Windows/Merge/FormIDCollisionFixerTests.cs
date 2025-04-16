using System.IO.Abstractions;
using Autofac;
using LibGit2Sharp;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.IO;
using Noggog.Testing.AutoFixture;
using Noggog.Testing.Extensions;
using Shouldly;
using Spriggit.Core;
using Spriggit.Engine.Merge;
using Spriggit.Engine.Services.Singletons;
using Spriggit.Tests.Utility;
using Spriggit.Yaml.Skyrim;
using Xunit;
using IContainer = Autofac.IContainer;

namespace Spriggit.Tests.Windows.Merge;

public class FormIDCollisionFixerTests
{
    private static readonly SpriggitMeta _meta = new(
        new SpriggitSource()
        {
            PackageName = "Spriggit.Yaml.Skyrim",
            Version = "1.2.3"
        },
        GameRelease.SkyrimSE);
    
    private IContainer GetContainer(
        SpriggitMeta meta,
        IFileSystem? fileSystem)
    {
        var testModule = new TestModule(fileSystem.GetOrDefault(),
            (meta, new EntryPoint()));
        var builder = new ContainerBuilder();
        builder.RegisterModule(testModule);
        var cont = builder.Build();
        return cont;
    }
    
    [Theory, MutagenModAutoData(GameRelease.SkyrimSE, TargetFileSystem.Real)]
    public async Task NothingToFix(
        SkyrimMod mod,
        Npc n1,
        ModKey modKey)
    {
        using var tmp = TempFolder.FactoryByAddedPath(Path.Combine("SpriggitUnitTests", "FormIdCollisionFixer", nameof(NothingToFix)), throwIfUnsuccessfulDisposal: false, deleteAfter: false, deleteBefore: true);

        var meta = new SpriggitMeta(
            new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Skyrim",
                Version = "1.2.3"
            },
            GameRelease.SkyrimSE);
        
        var repoPath = Path.Combine(tmp.Dir, "Repo");
        Repository.Init(repoPath);
        var signature = new Signature("me", "someone@gmail.com", DateTimeOffset.Now);
        using var repo = new Repository(repoPath);
        var spriggitModPath = Path.Combine(repoPath, "Spriggit");
        
        var modFolder = Path.Combine(tmp.Dir, "ModFolder");
        Directory.CreateDirectory(modFolder);
        var modPath = Path.Combine(modFolder, mod.ModKey.FileName);
        mod.WriteToBinary(modPath);

        var container = GetContainer(meta, null);
        var engine = container.Resolve<SpriggitEngine>();
        
        await engine.Serialize(
            modPath, 
            spriggitModPath, 
            null,
            postSerializeChecks: true,
            throwOnUnknown: true,
            meta: meta);

        var fixer = container.Resolve<FormIDCollisionFixer>();
        
        await fixer.DetectAndFixInternal<ISkyrimMod, ISkyrimModGetter>(
            spriggitModPath: spriggitModPath,
            dataPath: null,
            meta: new SpriggitModKeyMeta(meta.Source, GameRelease.SkyrimSE, modKey));

        var modFolder2 = Path.Combine(tmp.Dir, "ModFolder2");
        var modPath2 = Path.Combine(modFolder2, mod.ModKey.FileName);
        Directory.CreateDirectory(modFolder2);
        
        await engine.Deserialize(
            spriggitModPath,
            modPath2,
            dataPath: null,
            backupDays: 0,
            localize: null,
            source: meta.Source);

        var reimport = SkyrimMod.CreateFromBinary(modPath2, SkyrimRelease.SkyrimSE);
        reimport.EnumerateMajorRecords().ShouldHaveCount(1);
        reimport.Npcs.First().FormKey.ShouldBe(n1.FormKey);
    }

    [Theory, MutagenModAutoData(GameRelease.SkyrimSE, TargetFileSystem.Real)]
    public async Task SomethingToFix(
        SkyrimMod mod,
        Armor armor)
    {
        using var tmp = TempFolder.FactoryByAddedPath(Path.Combine("SpriggitUnitTests", "FormIdCollisionFixer", nameof(SomethingToFix)), throwIfUnsuccessfulDisposal: false, deleteAfter: false, deleteBefore: true);

        var modFolder = Path.Combine(tmp.Dir, "ModFolder");
        Directory.CreateDirectory(modFolder);
        var modPath = Path.Combine(modFolder, mod.ModKey.FileName);
        var repoPath = Path.Combine(tmp.Dir, "Repo");
        Repository.Init(repoPath);
        var signature = new Signature("me", "someone@gmail.com", DateTimeOffset.Now);
        using var repo = new Repository(repoPath);
        var spriggitModPath = Path.Combine(repoPath, "Spriggit");

        var container = GetContainer(_meta, null);
        var engine = container.Resolve<SpriggitEngine>();
        
        // Make initial content
        File.WriteAllText(Path.Combine(tmp.Dir, "Readme.md"), "Readme");
        mod.WriteToBinary(modPath);
        await engine.Serialize(
            modPath, spriggitModPath, null,
            postSerializeChecks: true,
            throwOnUnknown: true,
            meta: _meta);

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
            
            await engine.Serialize(
                modPath, spriggitModPath,
                dataPath: null,
                postSerializeChecks: true,
                throwOnUnknown: true,
                meta: _meta);
            
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
            
            await engine.Serialize(
                modPath, spriggitModPath,
                dataPath: null, 
                postSerializeChecks: true,
                throwOnUnknown: true,
                meta: _meta);
            
            Commands.Stage(repo, "*");
            repo.Commit("Npc", signature, signature, new CommitOptions());
            mod.Npcs.Clear();
        }
        
        // Merge
        repo.Merge(lhs, signature, new MergeOptions());
        
        var sut = container.Resolve<FormIDCollisionFixer>();
        
        await sut.DetectAndFixInternal<ISkyrimMod, ISkyrimModGetter>(
            spriggitModPath: spriggitModPath,
            dataPath: null,
            meta: new SpriggitModKeyMeta(_meta.Source, GameRelease.SkyrimSE, mod.ModKey));

        var metaPath = Path.Combine(spriggitModPath, SpriggitExternalMetaPersister.FileName);
        File.Exists(metaPath).ShouldBeTrue();
        
        var modFolder2 = Path.Combine(tmp.Dir, "ModFolder2");
        var modPath2 = Path.Combine(modFolder2, mod.ModKey.FileName);
        Directory.CreateDirectory(modFolder2);
        await engine.Deserialize(
            spriggitPluginPath: spriggitModPath,
            outputFile: modPath2,
            dataPath: null,
            backupDays: 0,
            localize: null,
            source: _meta.Source);
        
        var reimport = SkyrimMod.CreateFromBinary(modPath2, SkyrimRelease.SkyrimSE);
        reimport.EnumerateMajorRecords().ShouldHaveCount(3);
        reimport.Armors.Select(x => x.FormKey)
            .ShouldEqual(armor.FormKey);
        reimport.Npcs.Count.ShouldBe(1);
        reimport.Weapons.Count.ShouldBe(1);
        reimport.Npcs.First().FormKey.ShouldNotBe(reimport.Weapons.First().FormKey);
    }
}