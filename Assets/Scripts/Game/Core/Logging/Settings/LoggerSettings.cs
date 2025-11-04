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

            foreach (var module in modules.AsValueEnumerable().Where(m => m != null && m.enabled))
            {
                var attribute = module.GetType().GetCustomAttribute<LogModuleAttribute>();
                if (!isEditor && attribute?.EditorOnly == true)
                    continue;

                GameLogger.RegisterModule(module);
            }
        }

        private void EnsureModulesInitialized(IReflectionManager reflectionManager)
        {
            if (modules == null)
            {
                modules = new List<ALogModule>();
            }

            var moduleTypes = reflectionManager.GetDerivedTypes<ALogModule>();
            var isFirstTimeInit = modules.Count == 0;
            var existingTypes = modules.AsValueEnumerable().Where(m => m != null).Select(m => m.GetType()).ToHashSet();

            foreach (var moduleType in moduleTypes)
            {
                if (!existingTypes.Contains(moduleType.AsType()))
                {
                    try
                    {
                        var attribute = moduleType.GetCustomAttribute<LogModuleAttribute>();
                        var instance = Activator.CreateInstance(moduleType.AsType()) as ALogModule;

                        if (instance != null)
                        {
                            if (isFirstTimeInit)
                            {
                                instance.enabled = attribute?.DefaultEnabled ?? false;
                            }
                            else
                            {
                                instance.enabled = false;
                            }

                            modules.Add(instance);
                        }
                    }
                    catch (Exception ex)
                    {
                        GameLogger.Warning($"Failed to create log module {moduleType.Name}: {ex.Message}");
                    }
                }
            }
        }
    }
}