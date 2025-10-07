using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Json.Fallout4.Customizations.Sorting;

public class QuestAdapterCustomization : ICustomize<IQuestAdapterGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IQuestAdapterGetter> builder)
    {
        builder.SortList(x => x.Fragments)
            .ByField(x => x.Stage);
    }
}
