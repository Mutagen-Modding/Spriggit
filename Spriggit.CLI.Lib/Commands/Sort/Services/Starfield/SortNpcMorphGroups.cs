using Mutagen.Bethesda.Starfield;
using Noggog;
using Serilog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;

public class SortNpcMorphGroups : ISortSomething<IStarfieldMod, IStarfieldModGetter>
{
    private readonly ILogger _logger;

    public SortNpcMorphGroups(ILogger logger)
    {
        _logger = logger;
    }
    
    public bool HasWorkToDo(IStarfieldModGetter mod)
    {
        foreach (var npc in mod
                     .EnumerateMajorRecords<INpcGetter>())
        {
            var morphGroupNames = npc.FaceMorphs.SelectMany(x => x.MorphGroups).Select(x => x.MorphGroup).ToArray();
            if (!morphGroupNames.SequenceEqual(morphGroupNames.OrderBy(x => x)))
            {
                _logger.Information($"{npc} Morph Group Names sorting to be done.");
                return true;
            }
        }

        return false;
    }

    public void DoWork(IStarfieldMod mod)
    {
        foreach (var npc in mod
                     .EnumerateMajorRecords<INpc>())
        {
            foreach (var faceMorph in npc.FaceMorphs)
            {
                SortNpcFaceMorphs(faceMorph);
            }
        }
    }
    
    private void SortNpcFaceMorphs(INpcFaceMorph npcFaceMorph)
    {
        npcFaceMorph.MorphGroups.SetTo(
            npcFaceMorph.MorphGroups.ToArray().OrderBy(x => x.MorphGroup));
    }
}