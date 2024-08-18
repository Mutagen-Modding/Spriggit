using Mutagen.Bethesda;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.Engine.Services.Singletons;
using Xunit;

namespace Spriggit.Tests;

public class DataPathCheckerTests
{
    [Theory, MutagenAutoData]
    public void NoDataPathForNonSeparated(DataPathChecker sut)
    {
        sut.CheckDataPath(GameRelease.Oblivion, null);
    }
    
    [Theory, MutagenAutoData]
    public void MissingDataPathForNonSeparated(
        DirectoryPath dir,
        DataPathChecker sut)
    {
        sut.CheckDataPath(GameRelease.Oblivion, dir);
    }
    
    [Theory, MutagenAutoData]
    public void DataPathForNonSeparated(
        DirectoryPath existingDir,
        DataPathChecker sut)
    {
        sut.CheckDataPath(GameRelease.Oblivion, existingDir);
    }
    
    [Theory, MutagenAutoData]
    public void EmptyDataPathForNonSeparated(
        DirectoryPath existingDir,
        DataPathChecker sut)
    {
        sut.CheckDataPath(GameRelease.Oblivion, string.Empty);
    }
    
    [Theory, MutagenAutoData]
    public void NoDataPathForSeparated(DataPathChecker sut)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            sut.CheckDataPath(GameRelease.Starfield, null);
        });
    }
    
    [Theory, MutagenAutoData]
    public void MissingDataPathForSeparated(
        DirectoryPath dir,
        DataPathChecker sut)
    {
        Assert.Throws<DirectoryNotFoundException>(() =>
        {
            sut.CheckDataPath(GameRelease.Starfield, dir);
        });
    }
    
    [Theory, MutagenAutoData]
    public void DataPathForSeparated(
        DirectoryPath existingDir,
        DataPathChecker sut)
    {
        sut.CheckDataPath(GameRelease.Starfield, existingDir);
    }
    
    [Theory, MutagenAutoData]
    public void EmptyDataPathForSeparated(
        DirectoryPath existingDir,
        DataPathChecker sut)
    {
        sut.CheckDataPath(GameRelease.Starfield, string.Empty);
    }
}