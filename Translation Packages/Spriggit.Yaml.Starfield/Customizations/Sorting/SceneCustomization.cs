using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Yaml.Starfield.Customizations.Sorting;

public class SceneCustomization : ICustomize<ISceneScriptFragmentsGetter>
{
    public void CustomizeFor(ICustomizationBuilder<ISceneScriptFragmentsGetter> builder)
    {
        builder.SortList(x => x.PhaseFragments)
            .ByField(x => x.Index);
    }
}
