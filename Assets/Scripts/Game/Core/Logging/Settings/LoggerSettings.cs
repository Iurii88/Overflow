using Game.Core.Settings;
using Game.Core.Settings.Attributes;

namespace Game.Core.Logging.Settings
{
    [GameSettings("Logger")]
    public class LoggerSettings : AGameSettings
    {
        public LogLevel minimumLogLevel = LogLevel.Debug;

        public override void Apply()
        {
            GameLogger.MinimumLevel = minimumLogLevel;
        }
    }
}