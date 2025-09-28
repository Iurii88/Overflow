using System;
using Game.Core.Reflection.Attributes;

namespace Game.Core.Content.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IdentifierAttribute : ReflectionInjectAttribute
    {
        public readonly string identifier;

        public IdentifierAttribute(string identifier)
        {
            this.identifier = identifier;
        }
    }
}