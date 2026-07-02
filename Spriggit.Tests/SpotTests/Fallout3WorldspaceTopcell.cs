using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Fallout3;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Tests.Utility;
using Spriggit.Yaml.Fallout3;
using Xunit;

namespace Spriggit.Tests.SpotTests;

public class Fallout3WorldspaceTopcell
{
    [Theory, MutagenModAutoData(GameRelease.Fallout3)]
    public async Task WorldspaceTopcell(
        IFileSystem fileSystem,
        Fallout3Mod mod,
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

        var reimport = await TestFallout3Util.PassThrough(fileSystem, mod, existingDataFolder, spriggitFolder, otherModKey, entryPoint);
        reimport.Worldspaces.First().TopCell!.Temporary.Select(x => x.EditorID).First().ShouldBe(someEdid);
    }
}
