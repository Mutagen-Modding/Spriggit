using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Skyrim;

namespace Spriggit.Yaml.Skyrim.Customizations.Omit;

public class PlayerSkillsCustomization : ICustomize<IPlayerSkillsGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IPlayerSkillsGetter> builder)
    {
        builder.Omit(x => x.Unused);
        builder.Omit(x => x.Unused2);
    }
}
