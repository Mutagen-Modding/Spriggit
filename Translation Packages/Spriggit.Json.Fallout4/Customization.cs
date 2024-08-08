using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Json.Fallout4;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder
            .OmitLastModifiedData()
            .OmitTimestampData()
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
