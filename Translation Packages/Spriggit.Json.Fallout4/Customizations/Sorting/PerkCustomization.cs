using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Json.Fallout4.Customizations.Sorting;

public class PerkCustomization : ICustomize<IPerkGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IPerkGetter> builder)
    {
        builder.SortList(x => x.Effects)
            .ByField(x => x.Priority);
    }
}
