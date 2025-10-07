using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Yaml.Starfield.Customizations.Omit;

public class ModHeaderCustomization : ICustomize<IStarfieldModHeaderGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IStarfieldModHeaderGetter> builder)
    {
        builder.Omit(x => x.OverriddenForms);
    }
}
