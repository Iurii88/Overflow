using Game.Core.Logging.Modules.Attributes;

namespace Game.Core.Logging.Modules
{
    [LogModule(true)]
    public class LevelModule : ALogModule
    {
        public override string Process(LogLevel level, string message)
        {
            return $"[{level}] {message}";
        }
    }
}