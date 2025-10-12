namespace Game.Core.Logging
{
    public class LevelModule : ILogModule
    {
        public string Process(LogLevel level, string message)
        {
            return $"[{level}] {message}";
        }
    }
}