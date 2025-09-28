using System;
using Game.Core.Reflection.Attributes;

namespace Game.Core.Content.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentSchemaAttribute : ReflectionInjectAttribute
    {
        public readonly string schema;

        public ContentSchemaAttribute(string schema)
        {
            this.schema = schema;
        }
    }
}