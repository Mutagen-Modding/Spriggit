using Mutagen.Bethesda.Serialization.Customizations;

namespace Spriggit.Yaml.Starfield;

public class Customization : ICustomize
{
    public void Customize(ICustomizationBuilder builder)
    {
        builder
            .OmitLastModifiedData()
            .OmitTimestampData()
            .OmitUnknownGroupData()
            .OmitUnusedConditionDataFields()
            .FilePerRecord();
    }
}