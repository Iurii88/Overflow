using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Core.Logging.Modules;
using Game.Core.Logging.Modules.Attributes;
using Game.Core.Reflection;
using Game.Core.Settings;
using Game.Core.Settings.Attributes;
using UnityEngine;
using ZLinq;

namespace Game.Core.Logging.Settings
{
    [GameSettings("Logger")]
    public class LoggerSettings : AGameSettings
    {
        public LogLevel minimumLogLevel = LogLevel.Debug;

        [SerializeReference]
        public List<ALogModule> modules = new();

        public override void OnBeforeApply(IReflectionManager reflectionManager)
        {
            EnsureModulesInitialized(reflectionManager);
        }

        public override void Apply()
        {
            GameLogger.MinimumLevel = minimumLogLevel;
            GameLogger.ClearModules();

            var isEditor = Application.isEditor;
            foreach (var module in modules.AsValueEnumerable().Where(m => m != null))
            {
                if (!module.enabled)
                    continue;

                var attribute = module.GetType().GetCustomAttribute<LogModuleAttribute>();
                if (!isEditor && attribute?.EditorOnly == true)
                    continue;

                GameLogger.RegisterModule(module);
            }
        }

        private void EnsureModulesInitialized(IReflectionManager reflectionManager)
        {
            modules ??= new List<ALogModule>();

            var moduleTypes = reflectionManager.GetDerivedTypes<ALogModule>();
            var isFirstTimeInit = modules.Count == 0;
            var existingTypes = modules.AsValueEnumerable().Where(m => m != null).Select(m => m.GetType()).ToHashSet();

            foreach (var moduleType in moduleTypes)
            {
                if (existingTypes.Contains(moduleType.AsType()))
                    continue;

                try
                {
                    var attribute = moduleType.GetCustomAttribute<LogModuleAttribute>();
                    if (Activator.CreateInstance(moduleType.AsType()) is ALogModule instance)
                    {
                        if (isFirstTimeInit)
                            instance.enabled = attribute?.DefaultEnabled ?? false;
                        else
                            instance.enabled = false;

                        modules.Add(instance);
                    }
                }
                catch (Exception ex)
                {
                    GameLogger.Warning($"Failed to create log module {moduleType.Name}: {ex.Message}");
                }
            }
        }

#if UNITY_EDITOR
        private static readonly Dictionary<string, bool> LOGModuleFoldouts = new();

        public override bool DrawEditorField(FieldInfo field, object defaultInstance, bool isStandalonePreset)
        {
            if (field.Name != "modules")
                return false;

            DrawLogModulesField(isStandalonePreset);
            return true;
        }

        private void DrawLogModulesField(bool isStandalonePreset)
        {
            UnityEditor.EditorGUILayout.LabelField("Log Modules", UnityEditor.EditorStyles.boldLabel);
            UnityEditor.EditorGUILayout.Space(3);

            if (modules == null)
            {
                modules = new List<ALogModule>();
                return;
            }

            for (var i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module == null)
                    continue;

                var moduleAttribute = module.GetType().GetCustomAttribute<LogModuleAttribute>();
                if (isStandalonePreset && moduleAttribute?.EditorOnly == true)
                    continue;

                var moduleName = module.GetDisplayName();
                var foldoutKey = $"{GetType().Name}_{module.GetType().Name}";

                LOGModuleFoldouts.TryAdd(foldoutKey, false);

                UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);

                UnityEditor.EditorGUILayout.BeginHorizontal();

                var enabledContent = new GUIContent("", module.enabled ? "Enabled" : "Disabled");
                module.enabled = UnityEditor.EditorGUILayout.Toggle(enabledContent, module.enabled, GUILayout.Width(20));

                GUILayout.Space(-5);

                var foldoutStyle = new GUIStyle(UnityEditor.EditorStyles.foldout) { fontStyle = FontStyle.Bold };
                LOGModuleFoldouts[foldoutKey] = UnityEditor.EditorGUILayout.Foldout(LOGModuleFoldouts[foldoutKey], moduleName, true, foldoutStyle);

                UnityEditor.EditorGUILayout.EndHorizontal();

                if (LOGModuleFoldouts[foldoutKey])
                {
                    UnityEditor.EditorGUILayout.Space(3);
                    UnityEditor.EditorGUI.indentLevel++;

                    DrawModuleFields(module);

                    UnityEditor.EditorGUI.indentLevel--;
                    UnityEditor.EditorGUILayout.Space(3);
                }

                UnityEditor.EditorGUILayout.EndVertical();

                if (i < modules.Count - 1)
                    UnityEditor.EditorGUILayout.Space(2);
            }
        }

        private static void DrawModuleFields(ALogModule module)
        {
            var fields = module.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var defaultModule = Activator.CreateInstance(module.GetType()) as ALogModule;

            var hasVisibleFields = fields.AsValueEnumerable().Any(f => f.Name != "enabled");

            if (!hasVisibleFields)
            {
                UnityEditor.EditorGUILayout.LabelField("No configurable settings", UnityEditor.EditorStyles.centeredGreyMiniLabel);
                return;
            }

            foreach (var field in fields)
            {
                if (field.Name == "enabled")
                    continue;

                Game.Core.Settings.Editor.SettingsFieldDrawer.DrawField(field, module, defaultModule);
            }
        }
#endif
    }
}