using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Json.Starfield.Customizations.Omit;

public class ConditionCustomization : ICustomize<IConditionGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IConditionGetter> builder)
    {
        builder.Omit(x => x.Unknown1);
    }
}
