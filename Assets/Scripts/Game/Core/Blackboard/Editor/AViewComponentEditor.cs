using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.Core.Blackboard.Editor
{
    [CustomEditor(typeof(AViewComponent), true)]
    public class AViewComponentEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, bool> m_foldouts = new();
        private readonly Dictionary<string, int> m_selectedIndices = new();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw default blackboard field
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blackboard"));

            var viewComponent = target as AViewComponent;
            if (viewComponent == null || viewComponent.blackboard == null)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Assign a Blackboard to see parameter bindings.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Blackboard Parameters", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Find all BlackboardViewParameter fields
            var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var hasParameters = false;

            foreach (var field in fields)
            {
                if (!IsBlackboardViewParameter(field.FieldType))
                    continue;

                hasParameters = true;
                DrawBlackboardParameter(field, viewComponent);
            }

            if (!hasParameters)
                EditorGUILayout.HelpBox("No BlackboardViewParameter fields found in this component.", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private static bool IsBlackboardViewParameter(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(BlackboardViewParameter<>);
        }

        private void DrawBlackboardParameter(FieldInfo field, AViewComponent viewComponent)
        {
            var parameter = field.GetValue(target);
            if (parameter == null)
            {
                parameter = Activator.CreateInstance(field.FieldType);
                field.SetValue(target, parameter);
            }

            var foldoutKey = field.Name;
            m_foldouts.TryAdd(foldoutKey, true);

            // Get bound key
            var boundKeyProp = field.FieldType.GetProperty("BoundKey");
            var currentKey = boundKeyProp?.GetValue(parameter) as string;

            // Get available keys from blackboard
            var availableKeys = GetBlackboardKeys(viewComponent.blackboard, field.FieldType);

            // Main container
            var bgColor = EditorGUIUtility.isProSkin
                ? new Color(0.25f, 0.25f, 0.25f, 1f)
                : new Color(0.8f, 0.8f, 0.8f, 1f);

            var originalBg = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = originalBg;

            // Header with foldout
            EditorGUILayout.BeginHorizontal();

            var foldoutRect = GUILayoutUtility.GetRect(12, EditorGUIUtility.singleLineHeight, GUILayout.Width(12));
            m_foldouts[foldoutKey] = EditorGUI.Foldout(foldoutRect, m_foldouts[foldoutKey], GUIContent.none, true);

            var typeName = GetFriendlyTypeName(field.FieldType);
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.LabelField($"{field.Name}", labelStyle, GUILayout.Width(150));

            var typeStyle = new GUIStyle(EditorStyles.miniLabel);
            typeStyle.normal.textColor = EditorGUIUtility.isProSkin
                ? new Color(0.7f, 0.7f, 0.7f)
                : new Color(0.4f, 0.4f, 0.4f);
            EditorGUILayout.LabelField($"({typeName})", typeStyle, GUILayout.Width(80));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (m_foldouts[foldoutKey])
            {
                EditorGUILayout.Space(3);

                // Bind to key dropdown
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Bind to Key", GUILayout.Width(100));

                if (availableKeys.Count > 0)
                {
                    var currentIndex = string.IsNullOrEmpty(currentKey) ? 0 : Math.Max(0, availableKeys.IndexOf(currentKey));
                    m_selectedIndices.TryAdd(foldoutKey, currentIndex);

                    var newIndex = EditorGUILayout.Popup(m_selectedIndices[foldoutKey], availableKeys.ToArray());
                    if (newIndex != m_selectedIndices[foldoutKey])
                    {
                        m_selectedIndices[foldoutKey] = newIndex;
                        var newKey = newIndex == 0 ? "" : availableKeys[newIndex];
                        boundKeyProp?.SetValue(parameter, newKey);
                        EditorUtility.SetDirty(target);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("(No matching keys)", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndHorizontal();

                // Create new key button (only if key doesn't exist)
                var uppercaseFieldName = field.Name.ToUpper();
                var keyExists = availableKeys.Contains(uppercaseFieldName);

                if (!keyExists)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(104);

                    var buttonStyle = new GUIStyle(GUI.skin.button);
                    buttonStyle.fontSize = 11;

                    if (GUILayout.Button("+ Create New Key", buttonStyle, GUILayout.Height(22)))
                        CreateNewKey(field, viewComponent, parameter, boundKeyProp);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        private void CreateNewKey(FieldInfo field, AViewComponent viewComponent,
            object parameter, PropertyInfo boundKeyProp)
        {
            // Generate key name from field name (uppercase)
            var newKey = field.Name.ToUpper();

            // Force rebuild cache in blackboard before creating
            var rebuildCacheMethod = typeof(Blackboard).GetMethod("RebuildCache",
                BindingFlags.NonPublic | BindingFlags.Instance);
            rebuildCacheMethod?.Invoke(viewComponent.blackboard, null);

            var valueType = field.FieldType.GetGenericArguments()[0];
            var defaultValue = GetDefaultValue(valueType);

            // Use reflection to call Set<T>
            var setMethod = typeof(Blackboard).GetMethod("Set").MakeGenericMethod(valueType);
            setMethod.Invoke(viewComponent.blackboard, new[] { newKey, defaultValue });

            // Bind to new key
            boundKeyProp?.SetValue(parameter, newKey);

            // Update the selected index in dropdown
            var foldoutKey = field.Name;
            var availableKeys = GetBlackboardKeys(viewComponent.blackboard, field.FieldType);
            var newIndex = availableKeys.IndexOf(newKey);
            if (newIndex >= 0)
                m_selectedIndices[foldoutKey] = newIndex;

            EditorUtility.SetDirty(viewComponent.blackboard);
            EditorUtility.SetDirty(target);
        }

        private static List<string> GetBlackboardKeys(Blackboard blackboard, Type parameterType)
        {
            var keys = new List<string> { "(None)" };
            var valueType = parameterType.GetGenericArguments()[0];

            // Access private values field
            var valuesField = typeof(Blackboard).GetField("values",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (valuesField == null)
                return keys;

            var values = valuesField.GetValue(blackboard) as System.Collections.IList;
            if (values == null)
                return keys;

            foreach (var val in values)
            {
                if (val == null)
                    continue;

                var keyField = val.GetType().GetField("key");
                var key = keyField?.GetValue(val) as string;

                var getValueTypeMethod = val.GetType().GetMethod("GetValueType");
                var type = getValueTypeMethod?.Invoke(val, null) as Type;

                if (!string.IsNullOrEmpty(key) && type == valueType)
                {
                    keys.Add(key);
                }
            }

            return keys;
        }

        private static object GetDefaultValue(Type type)
        {
            if (type == typeof(string))
                return "";

            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        private static string GetFriendlyTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var genericType = type.GetGenericArguments()[0];
            return genericType.Name;
        }
    }
}