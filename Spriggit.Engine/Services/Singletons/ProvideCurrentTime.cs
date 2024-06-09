namespace Spriggit.Engine.Services.Singletons;

public interface IProvideCurrentTime
{
    DateTime Now { get; }
}

public class ProvideCurrentTime : IProvideCurrentTime
{
    public DateTime Now => DateTime.Now;
}