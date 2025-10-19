namespace Game.Core.Logging.Modules
{
    public class ColorModule : ILogModule
    {
        public string Process(LogLevel level, string message)
        {
            var color = level switch
            {
                LogLevel.Debug => "#888888",
                LogLevel.Log => "#00ff00",
                LogLevel.Warning => "#ffff00",
                LogLevel.Error => "#ff0000",
                _ => "white"
            };

            return $"<color={color}>{message}</color>";
        }
    }
}