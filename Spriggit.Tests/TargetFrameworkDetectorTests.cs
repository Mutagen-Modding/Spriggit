using System.IO.Abstractions;
using FluentAssertions;
using Noggog;
using Noggog.Testing.AutoFixture;
using Spriggit.Engine;
using Spriggit.Engine.Services.Singletons;
using Xunit;

namespace Spriggit.Tests;

public class TargetFrameworkDetectorTests
{
    [Theory, DefaultAutoData]
    public void Nothing(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        TargetFrameworkDirLocator sut)
    {
        sut.GetTargetFrameworkDir(existingDir, "net7.0").Should().BeNull();
    }
    
    [Theory, DefaultAutoData]
    public void SomeFramework(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        TargetFrameworkDirLocator sut)
    {
        var someFramework = new DirectoryPath(Path.Combine(existingDir, "lib", "netstandard2.0"));
        fileSystem.Directory.CreateDirectory(someFramework);
        sut.GetTargetFrameworkDir(existingDir, "net7.0").Should().Be(someFramework);
    }

    [Theory, DefaultAutoData]
    public void LatestNet(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        TargetFrameworkDirLocator sut)
    {
        var five = new DirectoryPath(Path.Combine(existingDir, "lib", "net5.0"));
        fileSystem.Directory.CreateDirectory(five);
        var six = new DirectoryPath(Path.Combine(existingDir, "lib", "net6.0"));
        fileSystem.Directory.CreateDirectory(six);
        sut.GetTargetFrameworkDir(existingDir, "net7.0").Should().Be(six);
    }

    [Theory, DefaultAutoData]
    public void PreferTargetFramework(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        TargetFrameworkDirLocator sut)
    {
        var five = new DirectoryPath(Path.Combine(existingDir, "lib", "net5.0"));
        fileSystem.Directory.CreateDirectory(five);
        var six = new DirectoryPath(Path.Combine(existingDir, "lib", "net6.0"));
        fileSystem.Directory.CreateDirectory(six);
        sut.GetTargetFrameworkDir(existingDir, "net5.0").Should().Be(five);
    }

    [Theory, DefaultAutoData]
    public void NetOverFramework(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        TargetFrameworkDirLocator sut)
    {
        var framework = new DirectoryPath(Path.Combine(existingDir, "lib", "net48"));
        fileSystem.Directory.CreateDirectory(framework);
        var net = new DirectoryPath(Path.Combine(existingDir, "lib", "net6.0"));
        fileSystem.Directory.CreateDirectory(net);
        sut.GetTargetFrameworkDir(existingDir, "net7.0").Should().Be(net);
    }
    
    [Theory, DefaultAutoData]
    public void StandardOverFramework(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        TargetFrameworkDirLocator sut)
    {
        var framework = new DirectoryPath(Path.Combine(existingDir, "lib", "net48"));
        fileSystem.Directory.CreateDirectory(framework);
        var standard = new DirectoryPath(Path.Combine(existingDir, "lib", "netstandard2.0"));
        fileSystem.Directory.CreateDirectory(standard);
        sut.GetTargetFrameworkDir(existingDir, "net7.0").Should().Be(standard);
    }
    
    [Theory, DefaultAutoData]
    public void NetOverStandard(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        TargetFrameworkDirLocator sut)
    {
        var framework = new DirectoryPath(Path.Combine(existingDir, "lib", "net48"));
        fileSystem.Directory.CreateDirectory(framework);
        var net = new DirectoryPath(Path.Combine(existingDir, "lib", "net6.0"));
        fileSystem.Directory.CreateDirectory(net);
        sut.GetTargetFrameworkDir(existingDir, "net7.0").Should().Be(net);
    }
    
    [Theory, DefaultAutoData]
    public void FavorNotWindows(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        TargetFrameworkDirLocator sut)
    {
        var net = new DirectoryPath(Path.Combine(existingDir, "lib", "net6.0"));
        fileSystem.Directory.CreateDirectory(net);
        var windows = new DirectoryPath(Path.Combine(existingDir, "lib", "net7.0-windows"));
        fileSystem.Directory.CreateDirectory(windows);
        sut.GetTargetFrameworkDir(existingDir, "net7.0").Should().Be(net);
    }
    
    [Theory, DefaultAutoData]
    public void FrameworkOverUnknown(
        IFileSystem fileSystem,
        DirectoryPath existingDir,
        TargetFrameworkDirLocator sut)
    {
        var framework = new DirectoryPath(Path.Combine(existingDir, "lib", "net48"));
        fileSystem.Directory.CreateDirectory(framework);
        var unknown = new DirectoryPath(Path.Combine(existingDir, "lib", "MonoAndroid10"));
        fileSystem.Directory.CreateDirectory(unknown);
        sut.GetTargetFrameworkDir(existingDir, "net7.0").Should().Be(framework);
    }
}