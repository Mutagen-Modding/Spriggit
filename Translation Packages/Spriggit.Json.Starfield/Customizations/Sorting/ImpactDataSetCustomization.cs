using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Json.Starfield.Customizations.Sorting;

public class ImpactDataSetCustomization : ICustomize<IImpactDataSetGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IImpactDataSetGetter> builder)
    {
        builder.SortList(x => x.Impacts)
            .ByField(x => x.Material.FormKey)
            .ThenByField(x => x.Impact.FormKey);
    }
}
