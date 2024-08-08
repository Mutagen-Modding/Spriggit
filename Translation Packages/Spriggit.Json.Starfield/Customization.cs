using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Json.Starfield;

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