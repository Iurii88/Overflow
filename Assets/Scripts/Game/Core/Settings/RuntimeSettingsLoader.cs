using System;
using Game.Core.Logging;
using Game.Core.Reflection;
using Game.Core.Settings.Attributes;
using ZLinq;

namespace Game.Core.Settings
{
    public static class RuntimeSettingsLoader
    {
        public static void LoadAllSettings(IReflectionManager reflectionManager)
        {
            GameSettingsManager.Load();

            var settingsTypes = reflectionManager.GetByAttribute<GameSettingsAttribute>();

            var settingsWithOrder = settingsTypes
                .AsValueEnumerable()
                .Select(typeInfo =>
                {
                    var attribute = typeInfo.GetCustomAttributes(typeof(GameSettingsAttribute), false)[0] as GameSettingsAttribute;
                    return (typeInfo, order: attribute?.Order ?? 0);
                })
                .OrderBy(x => x.order)
                .ToArray();

            foreach (var (typeInfo, _) in settingsWithOrder)
            {
                try
                {
                    var settings = GameSettingsManager.GetSetting(typeInfo.AsType());

                    if (settings != null)
                    {
                        settings.OnBeforeApply(reflectionManager);
                        settings.Apply();
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.Error($"Failed to apply settings for {typeInfo.Name}: {ex.Message}");
                }
            }

            GameLogger.Log("Runtime settings loaded successfully");
        }
    }
}