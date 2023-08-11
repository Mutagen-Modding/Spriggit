using Mutagen.Bethesda;
using Noggog.IO;
using Noggog.WorkEngine;
using Spriggit.Core;
using Spriggit.Yaml.Skyrim;

var ep = new EntryPoint();
using var tmp = TempFolder.FactoryByPath(@"C:\Users\Levia\Documents\SpriggitTests", deleteAfter: false);

await ep.Serialize(
    @"D:\Games\steamapps\common\Skyrim Special Edition\Data\Unofficial Skyrim Special Edition Patch.esp",
    tmp.Dir,
    GameRelease.SkyrimSE,
    workDropoff: new ParallelWorkDropoff(),
    fileSystem: null,
    streamCreator: null,
    new SpriggitSource()
    {
        PackageName = "SpriggitTester",
        Version = "Testing123"
    },
    CancellationToken.None);