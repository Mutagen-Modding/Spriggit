using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Json.Skyrim;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder.FilePerRecord();
    }
}