using System;
using System.Collections.Generic;
using Game.Core.Logging.Modules;
using ZLinq;

namespace Game.Core.Logging
{
    public static class GameLogger
    {
        public static LogLevel minimumLevel { get; set; } = LogLevel.Debug;

        private static readonly List<ILogModule> Modules = new();

        public static void Initialize(LogLevel level, params ILogModule[] modules)
        {
            minimumLevel = level;
            ClearModules();

            foreach (var module in modules)
                RegisterModule(module);
        }

        public static void RegisterModule(ILogModule module)
        {
            Modules.Add(module);
        }

        public static void UnregisterModule(ILogModule module)
        {
            Modules.Remove(module);
        }

        public static void ClearModules()
        {
            Modules.Clear();
        }

        public static void Debug(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Debug, message, context);
        }

        public static void Log(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Log, message, context);
        }

        public static void Warning(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Warning, message, context);
        }

        public static void Error(string message, UnityEngine.Object context = null)
        {
            Log(LogLevel.Error, message, context);
        }

        private static void Log(LogLevel level, string message, UnityEngine.Object context)
        {
            if (level < minimumLevel)
                return;

            var processedMessage = Modules.AsValueEnumerable().Aggregate(message, (current, module) => module.Process(level, current));

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Log:
                    UnityEngine.Debug.Log(processedMessage, context);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(processedMessage, context);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(processedMessage, context);
                    break;
                case LogLevel.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}