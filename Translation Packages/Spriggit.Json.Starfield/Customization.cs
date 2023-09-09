using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Json.Starfield;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder.FilePerRecord();
    }
}