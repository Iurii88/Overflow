namespace Game.Core.Logging.Modules
{
    public class LevelModule : ILogModule
    {
        public string Process(LogLevel level, string message)
        {
            return $"[{level}] {message}";
        }
    }
}