namespace Spriggit.Engine;

public interface IProvideCurrentTime
{
    DateTime Now { get; }
}

public class ProvideCurrentTime : IProvideCurrentTime
{
    public DateTime Now => DateTime.Now;
}