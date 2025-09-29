using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Yaml.Fallout4;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder
            .OmitLastModifiedData()
            .OmitTimestampData()
            .OmitUnknownGroupData()
            .FilePerRecord();
    }
}

public class ModHeaderCustomization : ICustomize<IFallout4ModHeaderGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IFallout4ModHeaderGetter> builder)
    {
        builder.Omit(x => x.OverriddenForms);
    }
}

public class ModHeaderStatsCustomization : ICustomize<IModStatsGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IModStatsGetter> builder)
    {
        builder.Omit(x => x.NextFormID);
        builder.Omit(x => x.NumRecords);
    }
}

public class ConditionCustomization : ICustomize<IConditionGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IConditionGetter> builder)
    {
        builder.Omit(x => x.Unknown1);
    }
}

public class CellCustomization : ICustomize<ICellGetter>
{
    public void CustomizeFor(ICustomizationBuilder<ICellGetter> builder)
    {
        builder.EmbedRecordsInSameFile(x => x.Temporary)
            .EmbedRecordsInSameFile(x => x.Persistent)
            .EmbedRecordsInSameFile(x => x.Landscape)
            .EmbedRecordsInSameFile(x => x.NavigationMeshes);
        builder.SortList(x => x.Persistent)
            .ByField(x => x.FormKey);
        builder.SortList(x => x.Temporary)
            .ByField(x => x.FormKey);
    }
}

public class WorldspaceCustomization : ICustomize<IWorldspaceGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IWorldspaceGetter> builder)
    {
        builder.EmbedRecordsInSameFile(x => x.TopCell);
    }
}

public class PerkCustomization : ICustomize<IPerkGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IPerkGetter> builder)
    {
        builder.SortList(x => x.Effects)
            .ByField(x => x.Priority);
    }
}

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

public class ScriptEntryCustomization : ICustomize<IScriptEntryGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IScriptEntryGetter> builder)
    {
        builder.SortList(x => x.Properties)
            .ByField(x => x.Name);
    }
}

public class HeadDataCustomization : ICustomize<IHeadDataGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IHeadDataGetter> builder)
    {
        builder.SortList(x => x.MorphGroups)
            .ByField(x => x.Name);
    }
}

public class ImpactDataSetCustomization : ICustomize<IImpactDataSetGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IImpactDataSetGetter> builder)
    {
        builder.SortList(x => x.Impacts)
            .ByField(x => x.Material.FormKey)
            .ThenByField(x => x.Impact.FormKey);
    }
}