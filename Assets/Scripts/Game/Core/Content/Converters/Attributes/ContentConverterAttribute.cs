using System;
using Game.Core.Reflection.Attributes;

namespace Game.Core.Content.Converters.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentConverterAttribute : ReflectionInjectAttribute
    {
    }
}