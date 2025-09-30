using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Skyrim;

namespace Spriggit.Json.Skyrim.Customizations.Sorting;

public class QuestAdapterCustomization : ICustomize<IQuestAdapterGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IQuestAdapterGetter> builder)
    {
        builder.SortList(x => x.Fragments)
            .ByField(x => x.Stage);
    }
}
