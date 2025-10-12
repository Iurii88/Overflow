using System;

namespace Game.Core.Reflection.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterAttribute : ReflectionInjectAttribute
    {
    }
}