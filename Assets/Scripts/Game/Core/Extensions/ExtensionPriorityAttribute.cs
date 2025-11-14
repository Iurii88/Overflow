using System;

namespace Game.Core.Extensions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ExtensionPriorityAttribute : Attribute
    {
        public int Priority { get; }

        public ExtensionPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
