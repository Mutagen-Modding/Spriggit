using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Yaml.Starfield;

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

public class ModHeaderCustomization : ICustomize<IStarfieldModHeaderGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IStarfieldModHeaderGetter> builder)
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

public class ScriptEntryCustomization : ICustomize<IScriptEntryGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IScriptEntryGetter> builder)
    {
        builder.SortList(x => x.Properties)
            .ByField(x => x.Name);
    }
}

public class NpcCustomization : ICustomize<INpcGetter>
{
    public void CustomizeFor(ICustomizationBuilder<INpcGetter> builder)
    {
        builder.SortList(x => x.MorphBlends)
            .ByField(x => x.BlendName);
        builder.SortList(x => x.ActorEffect)
            .ByField(x => x.FormKey);
        builder.SortList(x => x.Factions)
            .ByField(x => x.Faction.FormKey)
            .ThenByField(x => x.Rank);
        builder.SortList(x => x.Items)
            .ByField(x => x.Item.Item.FormKey)
            .ThenByField(x => x.Item.Count);
    }
}

public class NpcFaceMorphCustomization : ICustomize<INpcFaceMorphGetter>
{
    public void CustomizeFor(ICustomizationBuilder<INpcFaceMorphGetter> builder)
    {
        builder.SortList(x => x.MorphGroups)
            .ByField(x => x.MorphGroup);
    }
}

public class ChargenCustomization : ICustomize<IChargenGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IChargenGetter> builder)
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