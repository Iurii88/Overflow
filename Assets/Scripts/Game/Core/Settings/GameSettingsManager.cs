using System;
using System.IO;
using Game.Core.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Core.Settings
{
    public static class GameSettingsManager
    {
        private static readonly string EditorPresetPath = Path.Combine(Application.dataPath, "..", "ProjectSettings", "GameSettings_EditorPreset.json");
        private static readonly string StandalonePresetPath = Path.Combine(Application.persistentDataPath, "GameSettings.json");

        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        private static GameSettingsData m_data;

        private static string CurrentPresetPath
        {
            get
            {
#if UNITY_EDITOR
                return EditorPresetPath;
#else
                return StandalonePresetPath;
#endif
            }
        }

        public static string GetEditorPresetPath() => EditorPresetPath;
        public static string GetStandalonePresetPath() => StandalonePresetPath;

        static GameSettingsManager()
        {
            Load();
        }

        public static void Load()
        {
            if (File.Exists(CurrentPresetPath))
            {
                try
                {
                    var json = File.ReadAllText(CurrentPresetPath);
                    m_data = JsonConvert.DeserializeObject<GameSettingsData>(json, SerializerSettings);
                    GameLogger.Log($"Loaded settings from {CurrentPresetPath}");
                }
                catch (Exception ex)
                {
                    GameLogger.Error($"Failed to load game settings: {ex.Message}");
                    LoadDefaultSettings();
                }
            }
            else
            {
                LoadDefaultSettings();
            }
        }

        private static void LoadDefaultSettings()
        {
            GameLogger.Warning($"Preset not found at {CurrentPresetPath}. Creating new preset with default values.");
            m_data = new GameSettingsData();
            Save();
        }

        public static void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(m_data, SerializerSettings);
                var directory = Path.GetDirectoryName(CurrentPresetPath);
                if (!Directory.Exists(directory))
                    if (directory != null)
                        Directory.CreateDirectory(directory);

                File.WriteAllText(CurrentPresetPath, json);
                GameLogger.Log($"Game settings saved to {CurrentPresetPath}");
            }
            catch (Exception ex)
            {
                GameLogger.Error($"Failed to save game settings: {ex.Message}");
            }
        }

        public static T GetSetting<T>(string moduleName, T defaultValue = default)
        {
            if (!m_data.moduleSettings.TryGetValue(moduleName, out var value))
                return defaultValue;

            try
            {
                var json = JsonConvert.SerializeObject(value, SerializerSettings);
                return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
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

            if (!m_data.moduleSettings.TryGetValue(settingsKey, out var value))
                return Activator.CreateInstance(type) as AGameSettings;

            try
            {
                var json = JsonConvert.SerializeObject(value, SerializerSettings);
                return JsonConvert.DeserializeObject(json, type, SerializerSettings) as AGameSettings;
            }
            catch (Exception ex)
            {
                GameLogger.Warning($"Failed to deserialize setting '{settingsKey}': {ex.Message}");
                return Activator.CreateInstance(type) as AGameSettings;
            }
        }

        public static void SetSetting<T>(string moduleName, T value)
        {
            m_data.moduleSettings[moduleName] = value;
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

        public static void SavePreset(string presetPath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(m_data, SerializerSettings);
                var directory = Path.GetDirectoryName(presetPath);
                if (!Directory.Exists(directory) && directory != null)
                    Directory.CreateDirectory(directory);

                File.WriteAllText(presetPath, json);
            }
            catch (Exception ex)
            {
                GameLogger.Error($"Failed to save preset: {ex.Message}");
            }
        }

        public static void LoadPreset(string presetPath)
        {
            if (!File.Exists(presetPath))
            {
                GameLogger.Warning($"Preset file not found: {presetPath}. Creating new preset with default values.");
                m_data = new GameSettingsData();
                SavePreset(presetPath);
                return;
            }

            try
            {
                var json = File.ReadAllText(presetPath);
                m_data = JsonConvert.DeserializeObject<GameSettingsData>(json, SerializerSettings);
                GameLogger.Log($"Preset loaded from {presetPath}");
            }
            catch (Exception ex)
            {
                GameLogger.Error($"Failed to load preset: {ex.Message}");
            }
        }
    }
}