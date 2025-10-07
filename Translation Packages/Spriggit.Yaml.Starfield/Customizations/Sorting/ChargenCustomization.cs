using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Yaml.Starfield.Customizations.Sorting;

public class ChargenCustomization : ICustomize<IChargenGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IChargenGetter> builder)
    {
        builder.SortList(x => x.MorphGroups)
            .ByField(x => x.Name);
    }
}
