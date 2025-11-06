using System;
using UnityEngine;

namespace Game.Core.Content.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ContentSelectorAttribute : PropertyAttribute
    {
        public readonly Type contentType;

        public ContentSelectorAttribute(Type contentType)
        {
            this.contentType = contentType;
        }
    }
}
