namespace Game.Core.Logging
{
    public interface ILogModule
    {
        string Process(LogLevel level, string message);
    }
}