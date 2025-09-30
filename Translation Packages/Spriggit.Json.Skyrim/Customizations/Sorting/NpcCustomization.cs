using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Skyrim;

namespace Spriggit.Json.Skyrim.Customizations.Sorting;

public class NpcCustomization : ICustomize<INpcGetter>
{
    public void CustomizeFor(ICustomizationBuilder<INpcGetter> builder)
    {
        builder.SortList(x => x.ActorEffect)
            .ByField(x => x.FormKey);
        builder.SortList(x => x.Factions)
            .ByField(x => x.Faction.FormKey)
            .ThenByField(x => x.Rank);
        builder.SortList(x => x.Items)
            .ByField(x => x.Item.Item.FormKey)
            .ThenByField(x => x.Item.Count);
        builder.SortList(x => x.Attacks)
            .ByField(x => x.AttackEvent);
    }
}
