using Noggog;

namespace Spriggit.Core;

public class CurrentVersionsProvider
{
    public string? SpriggitVersion { get; }
    
    public CurrentVersionsProvider()
    {
        var vers = AssemblyVersions.For<CurrentVersionsProvider>();
        SpriggitVersion = vers.ProductVersion?.TrimStringFromEnd(".0").TrimStringFromEnd(".0");
    }
}