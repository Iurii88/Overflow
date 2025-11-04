using System;
using System.IO;
using Game.Core.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Core.Settings
{
    public static class GameSettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(Application.dataPath, "..", "ProjectSettings", "GameSettings.json");
        private static GameSettingsData m_data;

        static GameSettingsManager()
        {
            Load();
        }

        public static void Load()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    var json = File.ReadAllText(SettingsPath);
                    m_data = JsonConvert.DeserializeObject<GameSettingsData>(json);
                }
                catch (Exception ex)
                {
                    GameLogger.Error($"Failed to load game settings: {ex.Message}");
                    m_data = new GameSettingsData();
                }
            }
            else
            {
                m_data = new GameSettingsData();
            }
        }

        public static void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(m_data, Formatting.Indented);
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                    if (directory != null)
                        Directory.CreateDirectory(directory);

                File.WriteAllText(SettingsPath, json);
                GameLogger.Log("Game settings saved successfully");
            }
            catch (Exception ex)
            {
                GameLogger.Error($"Failed to save game settings: {ex.Message}");
            }
        }

        public static T GetSetting<T>(string moduleName, T defaultValue = default)
        {
            if (!m_data.moduleSettings.TryGetValue(moduleName, out var json))
                return defaultValue;

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                GameLogger.Warning($"Failed to deserialize setting '{moduleName}': {ex.Message}");
            }

            return defaultValue;
        }

        public static T GetSetting<T>() where T : AGameSettings, new()
        {
            return GetSetting(typeof(T).Name, new T());
        }

        public static AGameSettings GetSetting(Type type)
        {
            var settingsKey = type.Name;

            if (!m_data.moduleSettings.TryGetValue(settingsKey, out var json))
                return Activator.CreateInstance(type) as AGameSettings;

            try
            {
                return JsonConvert.DeserializeObject(json, type) as AGameSettings;
            }
            catch (Exception ex)
            {
                GameLogger.Warning($"Failed to deserialize setting '{settingsKey}': {ex.Message}");
                return Activator.CreateInstance(type) as AGameSettings;
            }
        }

        public static void SetSetting<T>(string moduleName, T value)
        {
            try
            {
                var json = JsonConvert.SerializeObject(value);
                m_data.moduleSettings[moduleName] = json;
            }
            catch (Exception ex)
            {
                GameLogger.Error($"Failed to serialize setting '{moduleName}': {ex.Message}");
            }
        }

        public static bool HasSetting(string moduleName)
        {
            return m_data.moduleSettings.ContainsKey(moduleName);
        }

        public static void RemoveSetting(string moduleName)
        {
            m_data.moduleSettings.Remove(moduleName);
        }

        public static void Clear()
        {
            m_data.moduleSettings.Clear();
        }
    }
}