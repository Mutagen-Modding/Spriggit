using Mutagen.Bethesda.Fallout4;
using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Yaml.Fallout4.Customizations.Omit;

public class ModHeaderCustomization : ICustomize<IFallout4ModHeaderGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IFallout4ModHeaderGetter> builder)
    {
        builder.Omit(x => x.OverriddenForms);
    }
}
