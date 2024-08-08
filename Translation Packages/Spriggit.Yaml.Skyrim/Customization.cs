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