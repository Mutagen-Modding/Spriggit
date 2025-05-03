using Mutagen.Bethesda.Fallout4;
using Noggog;
using Serilog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Fallout4;

public class SortMorphGroups : ISortSomething<IFallout4Mod, IFallout4ModGetter>
{
    private readonly ILogger _logger;

    public SortMorphGroups(ILogger logger)
    {
        _logger = logger;
    }
    
    public bool HasWorkToDo(IFallout4ModGetter mod)
    {
        return mod
            .EnumerateMajorRecords<IRaceGetter>()
            .Any(race =>
            {
                var headData = race.HeadData;
                if (HeadDataHasWorkToDo(headData?.Male))
                {
                    _logger.Information($"{race} Male Head Data sorting to be done.");
                    return true;
                }

                if (HeadDataHasWorkToDo(headData?.Female))
                {
                    _logger.Information($"{race} Female Head Data sorting to be done.");
                    return true;
                }

                return false;
            });
    }

    private bool HeadDataHasWorkToDo(IHeadDataGetter? headData)
    {
        if (headData == null) return false;
        var names = headData.MorphGroups.Select(x => x.Name).ToArray();
        if (!names.SequenceEqual(names.OrderBy(x => x)))
        {
            return true;
        }

        return false;
    }

    public void DoWork(IFallout4Mod mod)
    {
        foreach (var race in mod.EnumerateMajorRecords<IRace>())
        {
            SortHeadDataMorphGroups(race.HeadData?.Male);
            SortHeadDataMorphGroups(race.HeadData?.Female);
        }
    }
    
    private void SortHeadDataMorphGroups(IHeadData? headData)
    {
        if (headData == null) return;
        headData.MorphGroups.SetTo(
            headData.MorphGroups.ToArray().OrderBy(x => x.Name));
    }
}
