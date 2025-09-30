using Mutagen.Bethesda.Serialization.Customizations;
using Mutagen.Bethesda.Starfield;

namespace Spriggit.Json.Starfield;

public class VirtualMachineAdapterCustomization : ICustomize<IAVirtualMachineAdapterGetter>
{
    public void CustomizeFor(ICustomizationBuilder<IAVirtualMachineAdapterGetter> builder)
    {
        builder.SortList(x => x.Scripts)
            .ByField(x => x.Name);
    }
}
