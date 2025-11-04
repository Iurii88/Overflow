#if UNITY_EDITOR
using System;
using System.Reflection;
using Game.Core.Logging.Modules;
using UnityEditor;
using UnityEngine;

namespace Game.Core.Settings.Editor
{
    public static class SettingsFieldDrawer
    {
        public static bool DrawField(FieldInfo field, object instance, object defaultInstance, bool showResetButton = true)
        {
            var fieldType = field.FieldType;
            var fieldName = ObjectNames.NicifyVariableName(field.Name);
            var currentValue = field.GetValue(instance);
            var defaultValue = field.GetValue(defaultInstance);
            var isModified = !Equals(currentValue, defaultValue);

            EditorGUILayout.BeginHorizontal();

            var labelStyle = isModified ? EditorStyles.boldLabel : EditorStyles.label;
            var labelContent = new GUIContent(fieldName, isModified ? "Modified from default" : "");

            var handled = DrawFieldByType(fieldType, labelContent, labelStyle, currentValue, out var newValue);

            if (handled)
            {
                if (!Equals(currentValue, newValue))
                {
                    field.SetValue(instance, newValue);
                    GUI.changed = true;
                }

                if (showResetButton && isModified && GUILayout.Button("↺", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    field.SetValue(instance, defaultValue);
                    GUI.changed = true;
                }
            }
            else
            {
                EditorGUILayout.LabelField(fieldName, $"Unsupported type: {fieldType.Name}");
            }

            EditorGUILayout.EndHorizontal();

            return handled;
        }

        private static bool DrawFieldByType(Type fieldType, GUIContent labelContent, GUIStyle labelStyle, object currentValue, out object newValue)
        {
            newValue = currentValue;

            if (fieldType == typeof(int))
            {
                EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                newValue = EditorGUILayout.IntField((int)currentValue);
                return true;
            }

            if (fieldType == typeof(float))
            {
                EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                newValue = EditorGUILayout.FloatField((float)currentValue);
                return true;
            }

            if (fieldType == typeof(string))
            {
                EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                newValue = EditorGUILayout.TextField((string)currentValue);
                return true;
            }

            if (fieldType == typeof(bool))
            {
                newValue = EditorGUILayout.Toggle(labelContent, (bool)currentValue);
                return true;
            }

            if (fieldType.IsEnum)
            {
                EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                newValue = EditorGUILayout.EnumPopup((Enum)currentValue);
                return true;
            }

            if (fieldType == typeof(Vector2))
            {
                EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                newValue = EditorGUILayout.Vector2Field("", (Vector2)currentValue);
                return true;
            }

            if (fieldType == typeof(Vector3))
            {
                EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                newValue = EditorGUILayout.Vector3Field("", (Vector3)currentValue);
                return true;
            }

            if (fieldType == typeof(Color))
            {
                EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                newValue = EditorGUILayout.ColorField((Color)currentValue);
                return true;
            }

            if (fieldType == typeof(SerializableColor))
            {
                EditorGUILayout.PrefixLabel(labelContent, EditorStyles.label, labelStyle);
                var serializableColor = (SerializableColor)currentValue;
                var newColor = EditorGUILayout.ColorField(serializableColor.ToColor());
                newValue = new SerializableColor(newColor);
                return true;
            }

            return false;
        }
    }
}
#endif