using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Json.Starfield.Customizations.Sorting;

public partial class CellCustomization : ICustomize<ICellGetter>
{
    public void CustomizeFor(ICustomizationBuilder<ICellGetter> builder)
    {
        builder.EmbedRecordsInSameFile(x => x.Temporary)
            .EmbedRecordsInSameFile(x => x.Persistent)
            .EmbedRecordsInSameFile(x => x.NavigationMeshes);
        builder.SortList(x => x.Persistent)
            .ByField(x => x.FormKey);
        builder.SortList(x => x.Temporary)
            .ByField(x => x.FormKey);
    }
}
