using System;
using Game.Core.Reflection.Attributes;
using VContainer;

namespace Game.Features.Sessions.Attributes
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