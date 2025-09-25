using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Spriggit.CLI.Lib.Commands.Sort.Services.Skyrim;

public class SortLocations : ISortSomething<ISkyrimMod, ISkyrimModGetter>
{
    public bool HasWorkToDo(ISkyrimModGetter mod)
    {
        return mod.Locations
            .Any(location =>
            {
                var references = location.PersistentActorReferencesAdded?.ToArray();
                if (references == null) return false;
                if (references.Length <= 1) return false;

                var sorted = references
                    .OrderBy(x => x.Grid.X)
                    .ThenBy(x => x.Grid.Y)
                    .ThenBy(x => x.Actor.FormKey)
                    .ThenBy(x => x.Location.FormKey)
                    .ToArray();

                return !references.SequenceEqual(sorted);
            });
    }

    public void DoWork(ISkyrimMod mod)
    {
        foreach (var location in mod.Locations)
        {
            if (location.PersistentActorReferencesAdded?.Count <= 1) continue;

            location.PersistentActorReferencesAdded?.SetTo(
                location.PersistentActorReferencesAdded
                    .OrderBy(x => x.Grid.X)
                    .ThenBy(x => x.Grid.Y)
                    .ThenBy(x => x.Actor.FormKey)
                    .ThenBy(x => x.Location.FormKey));
        }
    }
}