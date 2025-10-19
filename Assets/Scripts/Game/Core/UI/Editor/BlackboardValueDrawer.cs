using System;
using UnityEditor;
using UnityEngine;

namespace Game.Core.ViewComponents.Editor
{
    [CustomPropertyDrawer(typeof(BlackboardVariable), true)]
    public class BlackboardValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the actual object
            var targetObject = GetTargetObjectOfProperty(property);
            if (targetObject == null)
            {
                EditorGUI.LabelField(position, label.text, "Null");
                EditorGUI.EndProperty();
                return;
            }

            var targetType = targetObject.GetType();

            // Draw key field
            var keyProp = property.FindPropertyRelative("key");
            var keyRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(keyRect, keyProp, new GUIContent("Key"));

            // Get value type
            var getValueTypeMethod = targetType.GetMethod("GetValueType");
            var valueType = getValueTypeMethod?.Invoke(targetObject, null) as Type;

            if (valueType != null)
            {
                // Try to find serialized value property
                var valueProp = property.FindPropertyRelative("value");

                if (valueProp != null)
                {
                    // Calculate proper height for value field
                    var valueHeight = EditorGUI.GetPropertyHeight(valueProp, true);
                    var valueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2,
                        position.width, valueHeight);

                    EditorGUI.BeginChangeCheck();

                    // Unity can serialize this type, use standard PropertyField
                    EditorGUI.PropertyField(valueRect, valueProp, new GUIContent("Value"), true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        property.serializedObject.ApplyModifiedProperties();

                        // Notify the Blackboard component
                        var blackboard = property.serializedObject.targetObject as Blackboard;
                        if (blackboard != null && keyProp != null)
                        {
                            blackboard.NotifyValueChangedInEditor(keyProp.stringValue);
                        }
                    }
                }
                else if (CustomBlackboardTypeDrawers.HasDrawer(valueType))
                {
                    var valueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2,
                        position.width, EditorGUIUtility.singleLineHeight);

                    // Custom drawer is registered for this type
                    var getObjectValueMethod = targetType.GetMethod("GetObjectValue");
                    var currentValue = getObjectValueMethod?.Invoke(targetObject, null);

                    EditorGUI.BeginChangeCheck();

                    if (CustomBlackboardTypeDrawers.TryDraw(valueRect, "Value", currentValue, valueType, out var newValue))
                    {
                        if (EditorGUI.EndChangeCheck())
                        {
                            // Set new value using reflection
                            var valueField = targetType.GetField("value");
                            if (valueField != null)
                            {
                                valueField.SetValue(targetObject, newValue);
                                property.serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(property.serializedObject.targetObject);

                                // Notify the Blackboard component
                                var blackboard = property.serializedObject.targetObject as Blackboard;
                                if (blackboard != null && keyProp != null)
                                {
                                    blackboard.NotifyValueChangedInEditor(keyProp.stringValue);
                                }
                            }
                        }
                    }
                    else
                    {
                        EditorGUI.EndChangeCheck();
                    }
                }
                else
                {
                    var valueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2,
                        position.width, EditorGUIUtility.singleLineHeight);

                    // Unity cannot serialize this type and no custom drawer registered
                    var errorStyle = new GUIStyle(EditorStyles.label);
                    errorStyle.normal.textColor = Color.yellow;
                    EditorGUI.LabelField(valueRect, "Value",
                        $"Type '{valueType.Name}' is not serializable by Unity", errorStyle);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Get the value property
            var valueProp = property.FindPropertyRelative("value");

            if (valueProp != null)
            {
                // Calculate height based on the value property
                var valueHeight = EditorGUI.GetPropertyHeight(valueProp, true);
                // Height for key + value fields + spacing
                return EditorGUIUtility.singleLineHeight + 2 + valueHeight;
            }

            // Default height for key + value fields
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }

        private object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal)).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }

        private object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, System.Reflection.BindingFlags.NonPublic |
                                            System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, System.Reflection.BindingFlags.NonPublic |
                                               System.Reflection.BindingFlags.Public |
                                               System.Reflection.BindingFlags.Instance |
                                               System.Reflection.BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }

            return null;
        }

        private object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null)
                return null;
            var enm = enumerable.GetEnumerator();
            using var enumerator = enm as IDisposable;

            for (var i = 0; i <= index; i++)
            {
                if (!enm.MoveNext())
                    return null;
            }

            return enm.Current;
        }
    }
}