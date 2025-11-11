using System;

namespace Game.Core.UI.Attributes
{
    public enum ButtonAlignment
    {
        Left,
        Center,
        Right
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string Label { get; }
        public float Height { get; }
        public float Width { get; }
        public ButtonAlignment Alignment { get; }

        public ButtonAttribute(string label = null, float height = 25f, float width = 0f, ButtonAlignment alignment = ButtonAlignment.Center)
        {
            Label = label;
            Height = height;
            Width = width;
            Alignment = alignment;
        }
    }
}
