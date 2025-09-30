using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Skyrim;

namespace Spriggit.Yaml.Skyrim;

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

public class ModHeaderCustomization : ICustomize<ISkyrimModHeaderGetter>
{
    public void CustomizeFor(ICustomizationBuilder<ISkyrimModHeaderGetter> builder)
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

public class ImpactDataSetCustomization : ICustomize<IImpactDataSetGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IImpactDataSetGetter> builder)
    {
        builder.SortList(x => x.Impacts)
            .ByField(x => x.Material.FormKey)
            .ThenByField(x => x.Impact.FormKey);
    }
}

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

public class VirtualMachineAdapterCustomization : ICustomize<IAVirtualMachineAdapterGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IAVirtualMachineAdapterGetter> builder)
    {
        builder.SortList(x => x.Scripts)
            .ByField(x => x.Name);
    }
}

public class QuestAdapterCustomization : ICustomize<IQuestAdapterGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IQuestAdapterGetter> builder)
    {
        builder.SortList(x => x.Fragments)
            .ByField(x => x.Stage);
    }
}