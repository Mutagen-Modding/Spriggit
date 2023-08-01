using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Serialization.Oblivion.Json;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder.FilePerRecord();
    }
}