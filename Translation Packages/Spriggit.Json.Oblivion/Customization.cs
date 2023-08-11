using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Json.Oblivion;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder.FilePerRecord();
    }
}