using Noggog;

namespace Spriggit.Core;

public class CurrentVersionsProvider
{
    public string? SpriggitVersion { get; }
    
    public CurrentVersionsProvider()
    {
        var vers = AssemblyVersions.For<CurrentVersionsProvider>();
        SpriggitVersion = vers.ProductVersion?.TrimEnd(".0").TrimEnd(".0");
    }
}