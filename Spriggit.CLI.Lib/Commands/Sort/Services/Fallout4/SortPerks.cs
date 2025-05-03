using Mutagen.Bethesda.Fallout4;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Fallout4;

public class SortPerks : ISortSomething<IFallout4Mod, IFallout4ModGetter>
{
    public bool HasWorkToDo(IFallout4ModGetter mod)
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

    public void DoWork(IFallout4Mod mod)
    {
        foreach (var perk in mod.Perks)
        {
            perk.Effects.SetTo(
                perk.Effects.ToArray().OrderBy(x => x.Priority));
        }
    }
}