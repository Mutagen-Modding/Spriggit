using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Yaml.Oblivion;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder.FilePerRecord();
    }
}