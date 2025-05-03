using Mutagen.Bethesda.Starfield;
using Noggog;
using Serilog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Starfield;

public class SortRaceMorphGroups : ISortSomething<IStarfieldMod, IStarfieldModGetter>
{
    private readonly ILogger _logger;

    public SortRaceMorphGroups(
        ILogger logger)
    {
        _logger = logger;
    }
    
    public bool HasWorkToDo(IStarfieldModGetter mod)
    {
        return mod
            .EnumerateMajorRecords<IRaceGetter>()
            .Any(race =>
            {
                var charGen = race.ChargenAndSkintones;
                if (ChargenHasWorkToDo(charGen?.Male))
                {
                    _logger.Information($"{race} Male CharGen has sorting to be done.");
                    return true;
                }

                if (ChargenHasWorkToDo(charGen?.Female))
                {
                    _logger.Information($"{race} Female CharGen has sorting to be done.");
                    return true;
                }

                return false;
            });
    }

    private bool ChargenHasWorkToDo(IChargenAndSkintonesGetter? charGen)
    {
        if (charGen?.Chargen == null) return false;
        var names = charGen.Chargen.MorphGroups.Select(x => x.Name).ToArray();
        if (!names.SequenceEqual(names.OrderBy(x => x)))
        {
            return true;
        }

        return false;
    }

    public void DoWork(IStarfieldMod mod)
    {
        foreach (var race in mod
                     .EnumerateMajorRecords<IRace>())
        {
            SortChargenMorphGroups(race.ChargenAndSkintones?.Male?.Chargen);
            SortChargenMorphGroups(race.ChargenAndSkintones?.Female?.Chargen);
        }
    }

    private void SortChargenMorphGroups(IChargen? item)
    {
        if (item == null) return;
        item.MorphGroups.SetTo(
            item.MorphGroups.ToArray().OrderBy(x => x.Name));
    }
}