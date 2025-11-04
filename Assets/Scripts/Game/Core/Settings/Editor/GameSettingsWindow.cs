using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Core.Logging;
using Game.Core.Logging.Modules;
using Game.Core.Logging.Modules.Attributes;
using Game.Core.Logging.Settings;
using Game.Core.Reflection.Editor;
using Game.Core.Settings.Attributes;
using UnityEditor;
using UnityEngine;
using ZLinq;

namespace Game.Core.Settings.Editor
{
    public class GameSettingsWindow : EditorWindow
    {
        private enum PresetType
        {
            Editor,
            Standalone
        }

        private class SettingsModule
        {
            public string name;
            public int order;
            public AGameSettings instance;
            public string settingsKey;
            public bool isFoldedOut = true;
        }

        private List<SettingsModule> m_modules;
        private Vector2 m_scrollPosition;
        private PresetType m_currentPreset = PresetType.Editor;
        private readonly Dictionary<string, bool> m_logModuleFoldouts = new();

        [MenuItem("Tools/Game Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameSettingsWindow>("Game Settings");
            window.minSize = new Vector2(800, 400);
            window.Show();
        }

        private void OnEnable()
        {
            LoadCurrentPreset();
        }

        private void LoadModules()
        {
            m_modules = new List<SettingsModule>();

            var reflectionManager = EditorReflectionService.GetOrCreateInstance();
            reflectionManager.Initialize();

            var moduleTypes = reflectionManager.GetByAttribute<GameSettingsAttribute>();

            foreach (var typeInfo in moduleTypes)
            {
                try
                {
                    var attribute = typeInfo.GetCustomAttributes(typeof(GameSettingsAttribute), false)[0] as GameSettingsAttribute;
                    var settingsType = typeInfo.AsType();

                    // Load saved settings (deserializes to concrete type)
                    var instance = GameSettingsManager.GetSetting(settingsType);

                    if (instance == null)
                        continue;

                    m_modules.Add(new SettingsModule
                    {
                        name = attribute.ModuleName,
                        order = attribute.Order,
                        instance = instance,
                        settingsKey = typeInfo.Name
                    });
                }
                catch (Exception ex)
                {
                    GameLogger.Error($"Failed to instantiate settings module {typeInfo.Name}: {ex.Message}");
                }
            }

            m_modules = m_modules.AsValueEnumerable()
                .OrderBy(x => x.order)
                .ThenBy(x => x.name)
                .ToList();

            foreach (var module in m_modules)
            {
                module.instance?.OnBeforeApply(reflectionManager);
            }
        }


        private void OnGUI()
        {
            if (m_modules == null || m_modules.Count == 0)
            {
                EditorGUILayout.HelpBox("No settings modules found. Create a class with [GameSettings] and [ReflectionInject] attributes inheriting from AGameSettings.", MessageType.Info);

                if (GUILayout.Button("Refresh"))
                {
                    LoadModules();
                }

                return;
            }

            DrawToolbar();
            DrawAllModules();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField("Preset:", GUILayout.Width(50));

            EditorGUI.BeginChangeCheck();
            m_currentPreset = (PresetType)EditorGUILayout.EnumPopup(m_currentPreset, EditorStyles.toolbarDropDown, GUILayout.Width(150));
            if (EditorGUI.EndChangeCheck())
            {
                LoadCurrentPreset();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset All", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("Reset Settings", "Are you sure you want to reset all settings to default?", "Yes", "No"))
                {
                    ResetAllModules();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private string GetCurrentPresetPath()
        {
            return m_currentPreset switch
            {
                PresetType.Editor => GameSettingsManager.GetEditorPresetPath(),
                PresetType.Standalone => GameSettingsManager.GetStandalonePresetPath(),
                _ => GameSettingsManager.GetEditorPresetPath()
            };
        }

        private void LoadCurrentPreset()
        {
            var presetPath = GetCurrentPresetPath();
            GameSettingsManager.LoadPreset(presetPath);
            LoadModules();
        }

        private void DrawAllModules()
        {
            EditorGUILayout.Space(5);
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            for (var i = 0; i < m_modules.Count; i++)
            {
                var module = m_modules[i];
                var modifiedCount = GetModifiedFieldsCount(module.instance);
                var hasModifications = modifiedCount > 0;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                var headerLabel = hasModifications ? $"{module.name} ({modifiedCount} modified)" : module.name;
                module.isFoldedOut = EditorGUILayout.BeginFoldoutHeaderGroup(module.isFoldedOut, headerLabel);

                if (GUILayout.Button("Reset", EditorStyles.miniButtonRight, GUILayout.Width(50)))
                {
                    if (EditorUtility.DisplayDialog("Reset Module", $"Reset '{module.name}' to default settings?", "Yes", "No"))
                    {
                        ResetModule(module);
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndFoldoutHeaderGroup();

                if (module.isFoldedOut)
                {
                    EditorGUILayout.Space(3);
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();

                    try
                    {
                        DrawSettingsFields(module.instance);
                    }
                    catch (Exception ex)
                    {
                        EditorGUILayout.HelpBox($"Error drawing module: {ex.Message}", MessageType.Error);
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        SaveModule(module);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(3);
                }

                EditorGUILayout.EndVertical();

                if (i < m_modules.Count - 1)
                {
                    EditorGUILayout.Space(2);
                    DrawSeparator();
                    EditorGUILayout.Space(2);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        private int GetModifiedFieldsCount(AGameSettings settings)
        {
            var fields = settings.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var defaultInstance = Activator.CreateInstance(settings.GetType()) as AGameSettings;
            var count = 0;

            foreach (var field in fields)
            {
                var currentValue = field.GetValue(settings);
                var defaultValue = field.GetValue(defaultInstance);

                if (!Equals(currentValue, defaultValue))
                    count++;
            }

            return count;
        }

        private void DrawSettingsFields(AGameSettings settings)
        {
            var fields = settings.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var defaultInstance = Activator.CreateInstance(settings.GetType()) as AGameSettings;

            foreach (var field in fields)
            {
                if (settings is LoggerSettings loggerSettings && field.Name == "modules")
                {
                    DrawLogModulesField(loggerSettings);
                    continue;
                }

                var fieldType = field.FieldType;
                var fieldName = ObjectNames.NicifyVariableName(field.Name);
                var currentValue = field.GetValue(settings);
                var defaultValue = field.GetValue(defaultInstance);
                var isModified = !Equals(currentValue, defaultValue);

                EditorGUILayout.BeginHorizontal();

                var labelStyle = isModified ? EditorStyles.boldLabel : EditorStyles.label;
                var labelContent = new GUIContent(fieldName, isModified ? "Modified from default" : "");

                if (fieldType == typeof(int))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.IntField((int)currentValue);
                    field.SetValue(settings, newValue);
                }
                else if (fieldType == typeof(float))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.FloatField((float)currentValue);
                    field.SetValue(settings, newValue);
                }
                else if (fieldType == typeof(string))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.TextField((string)currentValue);
                    field.SetValue(settings, newValue);
                }
                else if (fieldType == typeof(bool))
                {
                    var newValue = EditorGUILayout.Toggle(labelContent, (bool)currentValue);
                    field.SetValue(settings, newValue);
                }
                else if (fieldType.IsEnum)
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.EnumPopup((Enum)currentValue);
                    field.SetValue(settings, newValue);
                }
                else if (fieldType == typeof(Vector2))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.Vector2Field("", (Vector2)currentValue);
                    field.SetValue(settings, newValue);
                }
                else if (fieldType == typeof(Vector3))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.Vector3Field("", (Vector3)currentValue);
                    field.SetValue(settings, newValue);
                }
                else if (fieldType == typeof(Color))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.ColorField((Color)currentValue);
                    field.SetValue(settings, newValue);
                }
                else
                {
                    EditorGUILayout.LabelField(fieldName, $"Unsupported type: {fieldType.Name}");
                }

                if (isModified && GUILayout.Button("↺", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    field.SetValue(settings, defaultValue);
                    GUI.changed = true;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void SaveModule(SettingsModule module)
        {
            GameSettingsManager.SetSetting(module.settingsKey, module.instance);
            GameSettingsManager.SavePreset(GetCurrentPresetPath());
            module.instance.Apply();
        }

        private void ResetModule(SettingsModule module)
        {
            module.instance = Activator.CreateInstance(module.instance.GetType()) as AGameSettings;
            SaveModule(module);
        }

        private void ResetAllModules()
        {
            foreach (var module in m_modules)
            {
                try
                {
                    ResetModule(module);
                }
                catch (Exception ex)
                {
                    GameLogger.Error($"Failed to reset module {module.name}: {ex.Message}");
                }
            }

            GameLogger.Log("All settings reset successfully");
        }

        private void DrawLogModulesField(LoggerSettings loggerSettings)
        {
            EditorGUILayout.LabelField("Log Modules", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);

            if (loggerSettings.modules == null)
            {
                loggerSettings.modules = new List<ALogModule>();
                return;
            }

            var isStandalonePreset = m_currentPreset == PresetType.Standalone;

            for (var i = 0; i < loggerSettings.modules.Count; i++)
            {
                var module = loggerSettings.modules[i];
                if (module == null)
                    continue;

                var moduleAttribute = module.GetType().GetCustomAttribute<LogModuleAttribute>();
                if (isStandalonePreset && moduleAttribute?.EditorOnly == true)
                    continue;

                var moduleName = module.GetDisplayName();
                var foldoutKey = $"{loggerSettings.GetType().Name}_{module.GetType().Name}";

                if (!m_logModuleFoldouts.ContainsKey(foldoutKey))
                    m_logModuleFoldouts[foldoutKey] = false;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                var enabledContent = new GUIContent("", module.enabled ? "Enabled" : "Disabled");
                module.enabled = EditorGUILayout.Toggle(enabledContent, module.enabled, GUILayout.Width(20));

                GUILayout.Space(-5);

                var foldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
                m_logModuleFoldouts[foldoutKey] = EditorGUILayout.Foldout(m_logModuleFoldouts[foldoutKey], moduleName, true, foldoutStyle);

                EditorGUILayout.EndHorizontal();

                if (m_logModuleFoldouts[foldoutKey])
                {
                    EditorGUILayout.Space(3);
                    EditorGUI.indentLevel++;

                    DrawModuleFields(module);

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(3);
                }

                EditorGUILayout.EndVertical();

                if (i < loggerSettings.modules.Count - 1)
                    EditorGUILayout.Space(2);
            }
        }

        private void DrawModuleFields(ALogModule module)
        {
            var fields = module.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var defaultModule = Activator.CreateInstance(module.GetType()) as ALogModule;

            var hasVisibleFields = fields.AsValueEnumerable().Any(f => f.Name != "enabled");

            if (!hasVisibleFields)
            {
                EditorGUILayout.LabelField("No configurable settings", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            foreach (var field in fields)
            {
                if (field.Name == "enabled")
                    continue;

                var fieldType = field.FieldType;
                var fieldName = ObjectNames.NicifyVariableName(field.Name);
                var currentValue = field.GetValue(module);
                var defaultValue = field.GetValue(defaultModule);
                var isModified = !Equals(currentValue, defaultValue);

                EditorGUILayout.BeginHorizontal();

                var labelStyle = isModified ? EditorStyles.boldLabel : EditorStyles.label;
                var labelContent = new GUIContent(fieldName, isModified ? "Modified from default" : "");

                if (fieldType == typeof(int))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.IntField((int)currentValue);
                    field.SetValue(module, newValue);
                }
                else if (fieldType == typeof(float))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.FloatField((float)currentValue);
                    field.SetValue(module, newValue);
                }
                else if (fieldType == typeof(string))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.TextField((string)currentValue);
                    field.SetValue(module, newValue);
                }
                else if (fieldType == typeof(bool))
                {
                    var newValue = EditorGUILayout.Toggle(labelContent, (bool)currentValue);
                    field.SetValue(module, newValue);
                }
                else if (fieldType.IsEnum)
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.EnumPopup((Enum)currentValue);
                    field.SetValue(module, newValue);
                }
                else if (fieldType == typeof(Vector2))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.Vector2Field("", (Vector2)currentValue);
                    field.SetValue(module, newValue);
                }
                else if (fieldType == typeof(Vector3))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.Vector3Field("", (Vector3)currentValue);
                    field.SetValue(module, newValue);
                }
                else if (fieldType == typeof(Color))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var newValue = EditorGUILayout.ColorField((Color)currentValue);
                    field.SetValue(module, newValue);
                }
                else if (fieldType == typeof(SerializableColor))
                {
                    EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                    var serializableColor = (SerializableColor)currentValue;
                    var newColor = EditorGUILayout.ColorField(serializableColor.ToColor());
                    field.SetValue(module, new SerializableColor(newColor));
                }
                else
                {
                    EditorGUILayout.LabelField(fieldName, $"Unsupported type: {fieldType.Name}");
                }

                if (isModified && GUILayout.Button("↺", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    field.SetValue(module, defaultValue);
                    GUI.changed = true;
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}