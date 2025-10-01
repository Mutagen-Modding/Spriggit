using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Fallout4;

namespace Spriggit.Yaml.Fallout4.Customizations.Sorting;

public class SceneCustomization : ICustomize<ISceneScriptFragmentsGetter>
{
    public void CustomizeFor(ICustomizationBuilder<ISceneScriptFragmentsGetter> builder)
    {
        builder.SortList(x => x.PhaseFragments)
            .ByField(x => x.Index);
    }
}
