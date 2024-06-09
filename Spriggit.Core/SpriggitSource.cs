using Noggog;

namespace Spriggit.Core;

public class SpriggitSource
{
    public string PackageName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public override string ToString()
    {
        if (Version.IsNullOrEmpty())
        {
            return PackageName;
        }
        else
        {
            return $"{PackageName}.{Version}";
        }
    }
    
    protected bool Equals(SpriggitSource other)
    {
        return PackageName == other.PackageName && Version == other.Version;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SpriggitSource)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PackageName, Version);
    }

    public static bool operator ==(SpriggitSource? left, SpriggitSource? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SpriggitSource? left, SpriggitSource? right)
    {
        return !Equals(left, right);
    }
}