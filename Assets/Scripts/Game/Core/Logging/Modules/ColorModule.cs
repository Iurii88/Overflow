using Game.Core.Logging.Modules.Attributes;

namespace Game.Core.Logging.Modules
{
    [LogModule(true, true)]
    public class ColorModule : ALogModule
    {
        public SerializableColor debugColor = new(0.53f, 0.53f, 0.53f);
        public SerializableColor logColor = new(0f, 1f, 0f);
        public SerializableColor warningColor = new(1f, 1f, 0f);
        public SerializableColor errorColor = new(1f, 0f, 0f);

        public override string Process(LogLevel level, string message)
        {
            var color = level switch
            {
                LogLevel.Debug => debugColor.ToHtmlStringRGB(),
                LogLevel.Log => logColor.ToHtmlStringRGB(),
                LogLevel.Warning => warningColor.ToHtmlStringRGB(),
                LogLevel.Error => errorColor.ToHtmlStringRGB(),
                _ => "ffffff"
            };

            return $"<color=#{color}>{message}</color>";
        }
    }
}