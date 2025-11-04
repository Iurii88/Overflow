using System;
using UnityEngine;

namespace Game.Core.Logging.Modules
{
    [Serializable]
    public struct SerializableColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public SerializableColor(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public SerializableColor(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor()
        {
            return new Color(r, g, b, a);
        }

        public static implicit operator Color(SerializableColor serializableColor)
        {
            return serializableColor.ToColor();
        }

        public static implicit operator SerializableColor(Color color)
        {
            return new SerializableColor(color);
        }

        public string ToHtmlStringRGB()
        {
            return ColorUtility.ToHtmlStringRGB(ToColor());
        }
    }
}