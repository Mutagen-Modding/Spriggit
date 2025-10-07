using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Yaml.Fallout4.Customizations.Sorting;

public class HeadDataCustomization : ICustomize<IHeadDataGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IHeadDataGetter> builder)
    {
        builder.SortList(x => x.MorphGroups)
            .ByField(x => x.Name);
    }
}
