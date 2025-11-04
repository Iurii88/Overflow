using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Core.Logging;
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
            var isStandalonePreset = m_currentPreset == PresetType.Standalone;

            foreach (var field in fields)
            {
                if (settings.DrawEditorField(field, defaultInstance, isStandalonePreset))
                    continue;

                SettingsFieldDrawer.DrawField(field, settings, defaultInstance);
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
    }
}