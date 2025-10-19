namespace Game.Core.Logging.Modules
{
    public interface ILogModule
    {
        string Process(LogLevel level, string message);
    }
}