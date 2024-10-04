using System.IO.Abstractions;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Oblivion;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.Yaml.Oblivion;
using Xunit;

namespace Spriggit.Tests.SpotTests;

public class OblivionWorldspaceTopcell
{
    [Theory, MutagenModAutoData(GameRelease.Oblivion)]
    public async Task WorldspaceTopcell(
        IFileSystem fileSystem,
        OblivionMod mod,
        Random random,
        DirectoryPath existingDataFolder,
        DirectoryPath spriggitFolder,
        ModKey otherModKey,
        string someEdid,
        EntryPoint entryPoint)
    {
        var worldspace = mod.Worldspaces.AddNew();
        worldspace.TopCell = new Cell(mod)
        {
            Temporary = new ExtendedList<IPlaced>()
            {
                new PlacedNpc(mod)
                {
                    EditorID = someEdid
                }
            }
        };
        
        var reimport = await TestOblivionUtil.PassThrough(fileSystem, mod, existingDataFolder, spriggitFolder, otherModKey, entryPoint);
        reimport.Worldspaces.First().TopCell!.Temporary.Select(x => x.EditorID).First().Should().Be(someEdid);
    }
}