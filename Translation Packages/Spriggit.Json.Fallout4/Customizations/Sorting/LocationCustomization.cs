using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Json.Fallout4.Customizations.Sorting;

public class LocationCustomization : ICustomize<ILocationGetter>
{
    public void CustomizeFor(ICustomizationBuilder<ILocationGetter> builder)
    {
        builder.SortList(x => x.PersistentActorReferencesAdded)
            .ByField(x => x.Grid.X)
            .ThenByField(x => x.Grid.Y)
            .ThenByField(x => x.Actor.FormKey)
            .ThenByField(x => x.Location.FormKey);
        builder.SortList(x => x.UniqueActorReferencesAdded)
            .ByField(x => x.Ref.FormKey)
            .ThenByField(x => x.Actor.FormKey)
            .ThenByField(x => x.Actor.FormKey)
            .ThenByField(x => x.Location.FormKey);
        builder.SortList(x => x.LocationRefTypeReferencesAdded)
            .ByField(x => x.Grid.X)
            .ThenByField(x => x.Grid.Y)
            .ThenByField(x => x.Ref.FormKey)
            .ThenByField(x => x.Location.FormKey);
        builder.SortList(x => x.EnableParentReferencesAdded)
            .ByField(x => x.Grid.X)
            .ThenByField(x => x.Grid.Y)
            .ThenByField(x => x.Ref.FormKey)
            .ThenByField(x => x.Actor.FormKey);
    }
}
