using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Skyrim;

public class SortPerks : ISortSomething<ISkyrimMod, ISkyrimModGetter>
{
    public bool HasWorkToDo(ISkyrimModGetter mod)
    {
        return mod.Perks
            .Any(p =>
            {
                var effectIds = p.Effects.Select(e => e.Priority).ToArray();
                if (!effectIds.SequenceEqual(effectIds.OrderBy(x => x)))
                {
                    return true;
                }

                return false;
            });
    }

    public void DoWork(ISkyrimMod mod)
    {
        foreach (var perk in mod.Perks)
        {
            perk.Effects.SetTo(
                perk.Effects.ToArray().OrderBy(x => x.Priority));
        }
    }
}