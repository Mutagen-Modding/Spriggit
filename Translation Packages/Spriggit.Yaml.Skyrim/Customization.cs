using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Serialization.Skyrim.Yaml;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder.FilePerRecord();
    }
}