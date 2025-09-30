using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Json.Starfield.Customizations.Sorting;

public class NpcFaceMorphCustomization : ICustomize<INpcFaceMorphGetter>
{
    public void CustomizeFor(ICustomizationBuilder<INpcFaceMorphGetter> builder)
    {
        builder.SortList(x => x.MorphGroups)
            .ByField(x => x.MorphGroup);
    }
}
