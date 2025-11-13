using System;
using Game.Core.Logging;
using Game.Core.Reflection;
using Game.Core.Settings.Attributes;
using VContainer;
using VContainer.Unity;
using ZLinq;

namespace Game.Core.Settings
{
    public class RuntimeSettingsLoader : IStartable
    {
        [Inject]
        private IReflectionManager m_reflectionManager;

        public void Start()
        {
            GameSettingsManager.Load();

            var settingsTypes = m_reflectionManager.GetByAttribute<GameSettingsAttribute>();

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

                    if (settings == null)
                        continue;

                    settings.OnBeforeApply(m_reflectionManager);
                    settings.Apply();
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