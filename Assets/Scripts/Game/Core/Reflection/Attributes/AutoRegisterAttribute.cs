using System;
using VContainer;

namespace Game.Core.Reflection.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoRegisterAttribute : ReflectionInjectAttribute
    {
        public Lifetime Lifetime { get; } = Lifetime.Singleton;

        public AutoRegisterAttribute()
        {
        }

        public AutoRegisterAttribute(Lifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}