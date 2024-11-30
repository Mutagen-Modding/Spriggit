using System.IO.Abstractions;
using FluentAssertions;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.Core;
using Spriggit.Core.Services.Singletons;
using Spriggit.Engine.Services.Singletons;
using Xunit;

namespace Spriggit.Tests;

public class SpriggitFileLocatorTests
{
    [Theory, MutagenAutoData]
    public void Typical(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        SpriggitFileLocator fileLocator)
    {
        FilePath spriggitPath = Path.Combine(existingDir, SpriggitFileLocator.ConfigFileName);
        fileSystem.File.Create(spriggitPath);
        var subDir = Path.Combine(existingDir, "SubDir");
        fileSystem.Directory.CreateDirectory(subDir);
        fileLocator.LocateSpriggitConfigFile(subDir)
            .Should().Be(spriggitPath);
    }
    
    [Theory, MutagenAutoData]
    public void TargetFolderMissing(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        SpriggitFileLocator fileLocator)
    {
        FilePath spriggitPath = Path.Combine(existingDir, SpriggitFileLocator.ConfigFileName);
        fileSystem.File.Create(spriggitPath);
        var subDir = Path.Combine(existingDir, "SubDir");
        fileLocator.LocateSpriggitConfigFile(subDir)
            .Should().Be(spriggitPath);
    }
}