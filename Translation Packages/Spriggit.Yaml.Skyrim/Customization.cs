using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Yaml.Skyrim;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder.FilePerRecord();
    }
}