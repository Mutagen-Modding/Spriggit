using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;

namespace Spriggit.Engine.Collision;

public class FormIDCollisionDetector
{
    public Dictionary<FormKey, List<IMajorRecordGetter>> LocateCollisions(IModGetter mod)
    {
        Dictionary<FormKey, List<IMajorRecordGetter>> collisions = new();

        foreach (var majorRec in mod.EnumerateMajorRecords())
        {
            if (majorRec.FormKey.ModKey != mod.ModKey) continue;
            collisions.GetOrAdd(majorRec.FormKey).Add(majorRec);
        }

        foreach (var col in collisions)
        {
            if (col.Value.Count <= 1)
            {
                collisions.Remove(col.Key);
            }
        }

        return collisions;
    }
}