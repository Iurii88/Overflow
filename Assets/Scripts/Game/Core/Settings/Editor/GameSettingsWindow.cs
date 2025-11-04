using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Core.Reflection;
using Game.Core.Settings.Attributes;
using UnityEditor;
using UnityEngine;
using ZLinq;

namespace Game.Core.Settings.Editor
{
    public class GameSettingsWindow : EditorWindow
    {
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

        [MenuItem("Tools/Game Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameSettingsWindow>("Game Settings");
            window.minSize = new Vector2(800, 400);
            window.Show();
        }

        private void OnEnable()
        {
            LoadModules();
        }

        private void LoadModules()
        {
            m_modules = new List<SettingsModule>();

            var reflectionManager = new ReflectionManager();
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
                    Debug.LogError($"Failed to instantiate settings module {typeInfo.Name}: {ex.Message}");
                }
            }

            m_modules = m_modules.AsValueEnumerable()
                .OrderBy(x => x.order)
                .ThenBy(x => x.name)
                .ToList();
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
            
            EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel);

            if (GUILayout.Button("Reset All", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("Reset Settings", "Are you sure you want to reset all settings to default?", "Yes", "No"))
                {
                    ResetAllModules();
                }
            }

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                LoadModules();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawAllModules()
        {
            EditorGUILayout.Space(5);
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            for (int i = 0; i < m_modules.Count; i++)
            {
                var module = m_modules[i];

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();

                module.isFoldedOut = EditorGUILayout.BeginFoldoutHeaderGroup(module.isFoldedOut, module.name);

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

        private void DrawSettingsFields(AGameSettings settings)
        {
            var fields = settings.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                var fieldName = ObjectNames.NicifyVariableName(field.Name);
                var currentValue = field.GetValue(settings);

                if (fieldType == typeof(int))
                {
                    field.SetValue(settings, EditorGUILayout.IntField(fieldName, (int)currentValue));
                }
                else if (fieldType == typeof(float))
                {
                    field.SetValue(settings, EditorGUILayout.FloatField(fieldName, (float)currentValue));
                }
                else if (fieldType == typeof(string))
                {
                    field.SetValue(settings, EditorGUILayout.TextField(fieldName, (string)currentValue));
                }
                else if (fieldType == typeof(bool))
                {
                    field.SetValue(settings, EditorGUILayout.Toggle(fieldName, (bool)currentValue));
                }
                else if (fieldType.IsEnum)
                {
                    field.SetValue(settings, EditorGUILayout.EnumPopup(fieldName, (Enum)currentValue));
                }
                else if (fieldType == typeof(Vector2))
                {
                    field.SetValue(settings, EditorGUILayout.Vector2Field(fieldName, (Vector2)currentValue));
                }
                else if (fieldType == typeof(Vector3))
                {
                    field.SetValue(settings, EditorGUILayout.Vector3Field(fieldName, (Vector3)currentValue));
                }
                else if (fieldType == typeof(Color))
                {
                    field.SetValue(settings, EditorGUILayout.ColorField(fieldName, (Color)currentValue));
                }
                else
                {
                    EditorGUILayout.LabelField(fieldName, $"Unsupported type: {fieldType.Name}");
                }
            }
        }

        private void SaveModule(SettingsModule module)
        {
            GameSettingsManager.SetSetting(module.settingsKey, module.instance);
            GameSettingsManager.Save();
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
                    Debug.LogError($"Failed to reset module {module.name}: {ex.Message}");
                }
            }

            Debug.Log("All settings reset successfully");
        }
    }
}