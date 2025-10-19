using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.ViewComponents.Editor
{
    public static class CustomBlackboardTypeDrawers
    {
        public delegate object DrawerDelegate(Rect position, string label, object value, Type valueType);

        private static readonly Dictionary<Type, DrawerDelegate> Drawers = new();

        public static void Register<T>(DrawerDelegate drawer)
        {
            Drawers[typeof(T)] = drawer;
        }

        public static void Register(Type type, DrawerDelegate drawer)
        {
            Drawers[type] = drawer;
        }

        public static bool TryDraw(Rect position, string label, object value, Type valueType, out object newValue)
        {
            if (Drawers.TryGetValue(valueType, out var drawer))
            {
                newValue = drawer(position, label, value, valueType);
                return true;
            }

            newValue = null;
            return false;
        }

        public static bool HasDrawer(Type type)
        {
            return Drawers.ContainsKey(type);
        }

        public static void Unregister<T>()
        {
            Drawers.Remove(typeof(T));
        }

        public static void Unregister(Type type)
        {
            Drawers.Remove(type);
        }

        public static void Clear()
        {
            Drawers.Clear();
        }
    }
}