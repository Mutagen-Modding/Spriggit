using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Skyrim;

namespace Spriggit.Yaml.Skyrim.Customizations.Sorting;

public class RaceCustomization : ICustomize<IRaceGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IRaceGetter> builder)
    {
        builder.SortList(x => x.MovementTypeNames);
        builder.SortList(x => x.Attacks)
            .ByField(x => x.AttackEvent);
    }
}