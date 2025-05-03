using Mutagen.Bethesda.Starfield;
using Noggog;
using Serilog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;

public class SortMorphBlends : ISortSomething<IStarfieldMod, IStarfieldModGetter>
{
    private readonly ILogger _logger;

    public SortMorphBlends(ILogger logger)
    {
        _logger = logger;
    }
    
    public bool HasWorkToDo(IStarfieldModGetter mod)
    {
        foreach (var npc in mod
                     .EnumerateMajorRecords<INpcGetter>()
                     .AsParallel())
        {
            var morphBlendNames = npc.MorphBlends.Select(x => x.BlendName).ToArray();
            if (!morphBlendNames.SequenceEqual(morphBlendNames.OrderBy(x => x)))
            {
                _logger.Information($"{npc} Morph Blend Names sorting to be done.");
                return true;
            }
        }

        return false;
    }

    public void DoWork(IStarfieldMod mod)
    {
        foreach (var npc in mod
                     .EnumerateMajorRecords<INpc>()
                     .AsParallel())
        {
            npc.MorphBlends.SetTo(
                npc.MorphBlends.ToArray().OrderBy(x => x.BlendName));
        }
    }
}