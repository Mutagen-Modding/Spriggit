namespace Spriggit.Engine;

public enum FrameworkType
{
    Framework,
    CoreApp,
    Core,
    Standard,
    Net
}

public class GetFrameworkType
{
    public FrameworkType Get(
        ReadOnlySpan<char> span,
        out int number,
        out bool windows)
    {
        var orig = span;
        if (span.StartsWith("standard"))
        {
            number = default;
            windows = default;
            return FrameworkType.Standard;
        }

        if (span.StartsWith("coreapp"))
        {
            number = default;
            windows = default;
            return FrameworkType.CoreApp;
        }

        if (span.StartsWith("core"))
        {
            number = default;
            windows = default;
            return FrameworkType.Core;
        }

        var winIndex = span.IndexOf("-windows");
        if (winIndex != -1)
        {
            span = span.Slice(0, winIndex);
        }

        windows = winIndex != -1;

        if (!char.IsNumber(span[0])
            || !double.TryParse(span, out var d))
        {
            throw new ArgumentException(nameof(span), $"Could not parse framework type for {orig.ToString()}");
        }

        number = (int)d;
        if (number > 10)
        {
            return FrameworkType.Framework;
        }

        return FrameworkType.Net;
    }
}
