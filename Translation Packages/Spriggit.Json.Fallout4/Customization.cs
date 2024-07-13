using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Json.Fallout4;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder.FilePerRecord();
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
