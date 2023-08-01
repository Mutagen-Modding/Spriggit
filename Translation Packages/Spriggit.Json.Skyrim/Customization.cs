using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Serialization.Skyrim.Json;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder.FilePerRecord();
    }
}