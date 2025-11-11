using System.Linq;
using System.Reflection;
using Game.Core.UI.Attributes;
using UnityEditor;
using UnityEngine;

namespace Game.Core.UI.Editor
{
    public static class ButtonDrawerUtility
    {
        public static void DrawButtonMethods(Object target, Object[] targets = null)
        {
            var methods = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttribute<ButtonAttribute>() != null)
                .ToArray();

            if (methods.Length == 0)
                return;

            EditorGUILayout.Space(10);

            foreach (var method in methods)
            {
                var buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();
                var label = string.IsNullOrEmpty(buttonAttribute.Label) ? method.Name : buttonAttribute.Label;

                var layoutOptions = new System.Collections.Generic.List<GUILayoutOption>
                {
                    GUILayout.Height(buttonAttribute.Height)
                };

                if (buttonAttribute.Width > 0)
                {
                    layoutOptions.Add(GUILayout.Width(buttonAttribute.Width));
                }

                EditorGUILayout.BeginHorizontal();

                if (buttonAttribute.Alignment == Attributes.ButtonAlignment.Center || buttonAttribute.Alignment == Attributes.ButtonAlignment.Right)
                {
                    GUILayout.FlexibleSpace();
                }

                if (GUILayout.Button(label, layoutOptions.ToArray()))
                {
                    if (targets is { Length: > 0 })
                    {
                        foreach (var t in targets)
                        {
                            method.Invoke(t, null);
                            EditorUtility.SetDirty(t);
                        }
                    }
                    else
                    {
                        method.Invoke(target, null);
                        EditorUtility.SetDirty(target);
                    }
                }

                if (buttonAttribute.Alignment == Attributes.ButtonAlignment.Center || buttonAttribute.Alignment == Attributes.ButtonAlignment.Left)
                {
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
