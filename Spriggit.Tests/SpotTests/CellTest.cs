﻿using System.IO.Abstractions;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Starfield;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Shouldly;
using Spriggit.Tests.Utility;
using Spriggit.Yaml.Starfield;
using Xunit;

namespace Spriggit.Tests.SpotTests;

public class CellTest : SpotTestBase
{
    [Theory, MutagenModAutoData(GameRelease.Starfield)]
    public async Task CellTraversals(
        IFileSystem fileSystem,
        StarfieldMod mod,
        Random random,
        DirectoryPath existingDataFolder,
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
        
        var reimport = await TestStarfieldUtil.PassThrough(fileSystem, mod, existingDataFolder, spriggitFolder, otherModKey, entryPoint);
        reimport.Cells.First().SubBlocks.First().Cells.First().Traversals!.Count.ShouldBe(traversals.Count);
    }
}