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
using Noggog.WorkEngine;
using Spriggit.Core;
using Spriggit.Engine.Merge;
using Spriggit.Engine.Services.Singletons;
using Spriggit.Yaml.Starfield;
using Xunit;

namespace Spriggit.Tests.Merge;

public class MergeVersionSyncerTests
{
    public class Fixture
    {
        public FakeEntryPointCache EntryPointCache { get; }
        public MergeVersionSyncer Sut { get; }
        
        public Fixture(
            FakeEntryPointCache entryPointCache,
            GetMetaToUse getMetaToUse,
            SpriggitExternalMetaPersister metaPersister,
            GitFolderLocator gitFolderLocator)
        {
            EntryPointCache = entryPointCache;
            Sut = new MergeVersionSyncer(
                getMetaToUse,
                entryPointCache,
                metaPersister,
                gitFolderLocator);
        }
    }
    
    // [Theory, MutagenModAutoData(GameRelease.Starfield)]
    // public async Task NothingToFix(
    //     IFileSystem fileSystem,
    //     StarfieldMod mod,
    //     Npc n1,
    //     ModKey modKey,
    //     EntryPoint entryPoint,
    //     DirectoryPath modFolder,
    //     DirectoryPath modFolder2,
    //     DirectoryPath gitRootPath,
    //     DirectoryPath spriggitModPath,
    //     PostMergeVersionSyncer sut)
    // {
    //     var modPath = Path.Combine(modFolder, mod.ModKey.FileName);
    //     fileSystem.Directory.CreateDirectory(modFolder);
    //     mod.WriteToBinary(modPath, fileSystem: fileSystem);
    //     await entryPoint.Serialize(
    //         modPath, spriggitModPath, GameRelease.Starfield,
    //         null, fileSystem, null,
    //         new SpriggitSource()
    //         {
    //             PackageName = "Spriggit.Yaml.Starfield",
    //             Version = "1.0"
    //         },
    //         CancellationToken.None);
    //
    //     sut.DetectAndFix();
    //
    //     var modPath2 = Path.Combine(modFolder2, mod.ModKey.FileName);
    //     fileSystem.Directory.CreateDirectory(modFolder2);
    //     await entryPoint.Deserialize(
    //         inputPath: spriggitModPath,
    //         outputPath: modPath2,
    //         fileSystem: fileSystem,
    //         workDropoff: null,
    //         streamCreator: null,
    //         cancel: CancellationToken.None);
    //
    //     var reimport = StarfieldMod.CreateFromBinary(modPath2, StarfieldRelease.Starfield, fileSystem: fileSystem);
    //     reimport.EnumerateMajorRecords().Should().HaveCount(1);
    //     reimport.Npcs.First().FormKey.Should().Be(n1.FormKey);
    // }
    
    public class FakeEntryPoint : IEntryPoint
    {
        private readonly ModKey _modKey;
        private readonly bool _isOld;

        public FakeEntryPoint(
            ModKey modKey,
            bool isOld)
        {
            _modKey = modKey;
            _isOld = isOld;
        }
        
        public async Task Serialize(ModPath modPath, DirectoryPath outputDir, GameRelease release, IWorkDropoff? workDropoff,
            IFileSystem? fileSystem, ICreateStream? streamCreator, SpriggitSource meta, CancellationToken cancel)
        {
            if (_isOld)
            {
                throw new NotImplementedException();
            }
            fileSystem = fileSystem.GetOrDefault();
            modPath.ModKey.FileName.Should().Be(_modKey.FileName);
            fileSystem.File.ReadAllText(modPath).Should().Be("OldContent");
            fileSystem.File.WriteAllText(Path.Combine(outputDir, "Testing123"), "NewContent");
        }

        public async Task Deserialize(string inputPath, string outputPath,
            IWorkDropoff? workDropoff, IFileSystem? fileSystem,
            ICreateStream? streamCreator, CancellationToken cancel)
        {
            if (!_isOld)
            {
                throw new NotImplementedException();
            }
            fileSystem = fileSystem.GetOrDefault();
            fileSystem.Directory.Exists(inputPath).Should().BeTrue();
            Path.GetFileName(outputPath).Should().Be(_modKey.FileName);
            fileSystem.File.WriteAllText(outputPath, "OldContent");
        }

        public Task<SpriggitEmbeddedMeta?> TryGetMetaInfo(string inputPath, IWorkDropoff? workDropoff, IFileSystem? fileSystem, ICreateStream? streamCreator,
            CancellationToken cancel)
        {
            throw new NotImplementedException();
        }
    }

    [Theory, MutagenModAutoData(GameRelease.Starfield, TargetFileSystem.Real)]
    public async Task SomethingToFix(
        StarfieldMod mod,
        Armor armor,
        EntryPoint entryPoint,
        SpriggitExternalMetaPersister metaPersister,
        Fixture sut)
    {
        var v1 = new SpriggitEmbeddedMeta(
            new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "1.0"
            }, GameRelease.Starfield, mod.ModKey);
        var v2 = new SpriggitEmbeddedMeta(
            new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "2.0"
            }, GameRelease.Starfield, mod.ModKey);
        
        sut.EntryPointCache.RegisterFor(new SpriggitMeta(v1.Source, v1.Release), 
            new FakeEntryPoint(mod.ModKey, isOld: true));
        sut.EntryPointCache.RegisterFor(new SpriggitMeta(v2.Source, v2.Release), 
            new FakeEntryPoint(mod.ModKey, isOld: false));
        
        using var tmp = TempFolder.FactoryByAddedPath(Path.Combine("SpriggitUnitTests", "PostMergeVersionSyncer"), throwIfUnsuccessfulDisposal: false, deleteAfter: false);
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
        var weap = mod.Weapons.AddNew();
        mod.WriteToBinary(modPath);
        await entryPoint.Serialize(
            modPath, spriggitModPath, GameRelease.Starfield,
            null, null, null,
            v1.Source,
            CancellationToken.None);
        metaPersister.Persist(spriggitModPath, v1);

        Commands.Stage(repo, "*");
        repo.Commit("Initial", signature, signature, new CommitOptions());
        
        // Make branches
        var lhs = repo.CreateBranch("lhs");
        var rhs = repo.CreateBranch("rhs");

        // Setup lhs
        {
            Commands.Checkout(repo, lhs);
            weap.Name = "LHS";
            mod.WriteToBinary(modPath);
            
            await entryPoint.Serialize(
                modPath, spriggitModPath, GameRelease.Starfield,
                null, null, null,
                v1.Source,
                CancellationToken.None);
            metaPersister.Persist(spriggitModPath, v1);
            
            Commands.Stage(repo, "*");
            repo.Commit("v1", signature, signature, new CommitOptions());
        }

        // Setup rhs
        {
            Commands.Checkout(repo, rhs);
            weap.Name = null;
            weap.DistanceDeceleration = 1.4f;
            mod.WriteToBinary(modPath);
            
            await entryPoint.Serialize(
                modPath, spriggitModPath, GameRelease.Starfield,
                null, null, null,
                v2.Source,
                CancellationToken.None);
            metaPersister.Persist(spriggitModPath, v2);
            
            Commands.Stage(repo, "*");
            repo.Commit("v2", signature, signature, new CommitOptions());
            mod.Npcs.Clear();
        }
        
        // Merge
        repo.Merge(lhs, signature, new MergeOptions());
        
        await sut.Sut.DetectAndFix(
            spriggitModPath: spriggitModPath);

        File.ReadAllText(Path.Combine(spriggitModPath, "Testing123")).Should().Be("NewContent");
    }
}