using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Json.Starfield.Customizations.Sorting;

public class RaceCustomization : ICustomize<IRaceGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IRaceGetter> builder)
    {
        builder.SortList(x => x.MovementTypeNames)
            .ByField(x => x);
        builder.SortList(x => x.Attacks)
            .ByField(x => x.AttackEvent);
    }
}
