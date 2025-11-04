using System;
using Game.Core.Reflection.Attributes;

namespace Game.Core.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GameSettingsAttribute : ReflectionInjectAttribute
    {
        public string ModuleName { get; }
        public int Order { get; }

        public GameSettingsAttribute(string moduleName, int order = 0)
        {
            ModuleName = moduleName;
            Order = order;
        }
    }
}