using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Skyrim;

namespace Spriggit.Yaml.Skyrim.Customizations.Sorting;

public class SceneCustomization : ICustomize<ISceneScriptFragmentsGetter>
{
    public void CustomizeFor(ICustomizationBuilder<ISceneScriptFragmentsGetter> builder)
    {
        builder.SortList(x => x.PhaseFragments)
            .ByField(x => x.Index);
    }
}