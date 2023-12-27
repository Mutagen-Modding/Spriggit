using System.IO.Abstractions;
using FluentAssertions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Spriggit.Core;
using Spriggit.Yaml.Starfield;
using Xunit;

namespace Spriggit.Tests.SpotTests;

public class CellTest
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task CellTraversals(
        IFileSystem fileSystem,
        StarfieldMod mod,
        Random random,
        DirectoryPath dataFolder,
        DirectoryPath spriggitFolder,
        ModKey otherModKey,
        EntryPoint entryPoint)
    {
        TraversalReference MakeTraversal()
        {
            return new TraversalReference()
            {
                From = new P3Float()
                {
                    X = random.NextSingle(),
                    Y = random.NextSingle(),
                    Z = random.NextSingle(),
                },
                To = new P3Float()
                {
                    X = random.NextSingle(),
                    Y = random.NextSingle(),
                    Z = random.NextSingle(),
                },
                UnknownVector = new P3Float()
                {
                    X = random.NextSingle(),
                    Y = random.NextSingle(),
                    Z = random.NextSingle(),
                }
            };
        }

        var traversals = new ExtendedList<TraversalReference>();
        for (int i = 0; i < 300; i++)
        {
            traversals.Add(MakeTraversal());
        }

        mod.Cells.Add(new CellBlock()
        {
            BlockNumber = 1,
            SubBlocks = new ExtendedList<CellSubBlock>()
            {
                new CellSubBlock()
                {
                    BlockNumber = 2,
                    Cells = new ExtendedList<Cell>()
                    {
                        new Cell(mod)
                        {
                            Traversals = traversals
                        }
                    }
                }
            }
        });
        
        var modPath = new ModPath(Path.Combine(dataFolder, mod.ModKey.ToString()));
        fileSystem.Directory.CreateDirectory(dataFolder);
        mod.WriteToBinaryParallel(modPath, fileSystem: fileSystem);
        await entryPoint.Serialize(modPath, spriggitFolder, GameRelease.Starfield, workDropoff: null, fileSystem: fileSystem,
            streamCreator: null, new SpriggitSource()
            {
                PackageName = "Spriggit.Yaml.Starfield",
                Version = "Test"
            }, CancellationToken.None);
        var modPath2 = Path.Combine(dataFolder, otherModKey.ToString());
        await entryPoint.Deserialize(spriggitFolder, modPath2, workDropoff: null, fileSystem: fileSystem,
            streamCreator: null, CancellationToken.None);
        var reimport = StarfieldMod.CreateFromBinaryOverlay(modPath2, StarfieldRelease.Starfield, fileSystem: fileSystem);
        reimport.Cells.First().SubBlocks.First().Cells.First().Traversals!.Count.Should().Be(traversals.Count);
    }
}