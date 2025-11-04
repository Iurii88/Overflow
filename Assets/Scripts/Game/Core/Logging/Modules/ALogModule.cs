using System;
using Game.Core.Reflection.Attributes;

namespace Game.Core.Logging.Modules
{
    [Serializable]
    [ReflectionInject]
    public abstract class ALogModule : ILogModule
    {
        public bool enabled = true;

        public abstract string Process(LogLevel level, string message);

        public virtual string GetDisplayName()
        {
            var typeName = GetType().Name;
            return typeName.EndsWith("Module") ? typeName[..^6] : typeName;
        }
    }
}