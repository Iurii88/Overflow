using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.Core.UI.Editor
{
    [CustomEditor(typeof(AViewComponent), true)]
    public class AViewComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcludingBlackboardParameters();

            var viewComponent = target as AViewComponent;
            if (viewComponent == null || viewComponent.blackboard == null)
            {
                EditorGUILayout.Space(5);
                DrawBlackboardMissingButtons(viewComponent);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Blackboard Parameters", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

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

        private void DrawBlackboardMissingButtons(AViewComponent viewComponent)
        {
            EditorGUILayout.HelpBox("No Blackboard assigned.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Find in Parent", GUILayout.Height(30)))
            {
                if (viewComponent != null)
                {
                    var blackboard = viewComponent.GetComponentInParent<Blackboard>();
                    if (blackboard != null)
                    {
                        var blackboardProperty = serializedObject.FindProperty("blackboard");
                        if (blackboardProperty != null)
                        {
                            blackboardProperty.objectReferenceValue = blackboard;
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Not Found", "No Blackboard found in parent hierarchy.", "OK");
                    }
                }
            }

            if (GUILayout.Button("Create New", GUILayout.Height(30)))
            {
                if (viewComponent != null)
                {
                    var blackboard = viewComponent.gameObject.AddComponent<Blackboard>();
                    var blackboardProperty = serializedObject.FindProperty("blackboard");
                    if (blackboardProperty != null)
                    {
                        blackboardProperty.objectReferenceValue = blackboard;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPropertiesExcludingBlackboardParameters()
        {
            var prop = serializedObject.GetIterator();
            var enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (prop.name == "m_Script")
                    continue;

                var field = target.GetType().GetField(prop.name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null && IsBlackboardViewParameter(field.FieldType))
                    continue;

                EditorGUILayout.PropertyField(prop, true);
            }
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
                EditorUtility.SetDirty(target);
            }

            var fieldProperty = serializedObject.FindProperty(field.Name);
            if (fieldProperty == null)
                return;

            var boundKeyProperty = fieldProperty.FindPropertyRelative("boundKey");
            if (boundKeyProperty == null)
                return;

            var currentKey = boundKeyProperty.stringValue;

            var availableKeys = GetBlackboardKeys(viewComponent.blackboard, field.FieldType);

            var bgColor = EditorGUIUtility.isProSkin
                ? new Color(0.25f, 0.25f, 0.25f, 1f)
                : new Color(0.8f, 0.8f, 0.8f, 1f);

            var originalBg = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = originalBg;

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(10);

            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.fontStyle = FontStyle.Bold;

            var typeName = GetFriendlyTypeName(field.FieldType);

            EditorGUILayout.LabelField($"{field.Name} ({typeName})", labelStyle, GUILayout.MinWidth(100), GUILayout.MaxWidth(180));

            EditorGUILayout.LabelField("→", GUILayout.Width(15));

            var isKeyValid = !string.IsNullOrEmpty(currentKey) && currentKey != "(None)" && availableKeys.Contains(currentKey);

            if (availableKeys.Count > 0)
            {
                var currentIndex = isKeyValid ? availableKeys.IndexOf(currentKey) : 0;

                var newIndex = EditorGUILayout.Popup(currentIndex, availableKeys.ToArray(), GUILayout.MinWidth(80), GUILayout.MaxWidth(120));
                if (newIndex != currentIndex)
                {
                    var newKey = newIndex == 0 ? "" : availableKeys[newIndex];
                    boundKeyProperty.stringValue = newKey;
                }
            }
            else
            {
                EditorGUILayout.LabelField("(No keys)", EditorStyles.miniLabel, GUILayout.MinWidth(80), GUILayout.MaxWidth(100));
            }

            // Show [+] button when: no valid key is selected OR when there are no keys at all
            if (!isKeyValid || availableKeys.Count <= 1) // Count <= 1 because "(None)" is always in the list
            {
                if (GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(18)))
                {
                    var uppercaseFieldName = field.Name.ToUpper();
                    KeyNameInputWindow.Show(uppercaseFieldName, newKeyName =>
                    {
                        if (!string.IsNullOrWhiteSpace(newKeyName))
                        {
                            CreateNewKey(field, viewComponent, boundKeyProperty, newKeyName);
                        }
                    });
                }
            }

            GUILayout.FlexibleSpace();

            var currentValue = GetCurrentBlackboardValue(viewComponent.blackboard, currentKey);
            if (currentValue != null)
            {
                var valueStyle = new GUIStyle(EditorStyles.miniLabel);
                valueStyle.normal.textColor = EditorGUIUtility.isProSkin
                    ? new Color(0.5f, 0.8f, 0.5f)
                    : new Color(0.2f, 0.5f, 0.2f);
                EditorGUILayout.LabelField($"= {currentValue}", valueStyle, GUILayout.MinWidth(50));
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        private void CreateNewKey(FieldInfo field, AViewComponent viewComponent,
            SerializedProperty boundKeyProperty, string newKey)
        {
            if (string.IsNullOrWhiteSpace(newKey))
                return;

            var rebuildCacheMethod = typeof(Blackboard).GetMethod("RebuildCache",
                BindingFlags.NonPublic | BindingFlags.Instance);
            rebuildCacheMethod?.Invoke(viewComponent.blackboard, null);

            var valueType = field.FieldType.GetGenericArguments()[0];
            var defaultValue = GetDefaultValue(valueType);

            var setMethod = typeof(Blackboard).GetMethod("Set").MakeGenericMethod(valueType);
            setMethod.Invoke(viewComponent.blackboard, new[] { newKey, defaultValue });

            boundKeyProperty.stringValue = newKey;

            EditorUtility.SetDirty(viewComponent.blackboard);
        }

        private static string GetCurrentBlackboardValue(Blackboard blackboard, string key)
        {
            if (blackboard == null || string.IsNullOrEmpty(key))
                return null;

            var valuesField = typeof(Blackboard).GetField("values",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (valuesField == null)
                return null;

            var values = valuesField.GetValue(blackboard) as System.Collections.IList;
            if (values == null)
                return null;

            foreach (var val in values)
            {
                if (val == null)
                    continue;

                var keyField = val.GetType().GetField("key");
                var valueKey = keyField?.GetValue(val) as string;

                if (valueKey == key)
                {
                    var valueField = val.GetType().GetField("value");
                    var value = valueField?.GetValue(val);

                    if (value == null)
                        return "null";

                    var valueStr = value.ToString();
                    if (valueStr.Length > 30)
                        return valueStr.Substring(0, 27) + "...";

                    return valueStr;
                }
            }

            return null;
        }

        private static List<string> GetBlackboardKeys(Blackboard blackboard, Type parameterType)
        {
            var keys = new List<string> { "(None)" };
            var valueType = parameterType.GetGenericArguments()[0];

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

                if ((!string.IsNullOrEmpty(key) && type == valueType) || valueType == typeof(object))
                    keys.Add(key);
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

    public class KeyNameInputWindow : EditorWindow
    {
        private string m_keyName;
        private Action<string> m_onConfirm;
        private bool m_focusTextField;

        public static void Show(string defaultName, Action<string> onConfirm)
        {
            var window = GetWindow<KeyNameInputWindow>(true, "Create New Key", true);
            window.m_keyName = defaultName;
            window.m_onConfirm = onConfirm;
            window.m_focusTextField = true;
            window.minSize = new Vector2(300, 100);
            window.maxSize = new Vector2(400, 100);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Enter the name for the new Blackboard key:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(5);

            GUI.SetNextControlName("KeyNameField");
            m_keyName = EditorGUILayout.TextField("Key Name:", m_keyName);

            if (m_focusTextField)
            {
                EditorGUI.FocusTextInControl("KeyNameField");
                m_focusTextField = false;
            }

            // Handle Enter key
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                Confirm();
                Event.current.Use();
            }

            // Handle Escape key
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
                Event.current.Use();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create", GUILayout.Width(80), GUILayout.Height(25)))
            {
                Confirm();
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(80), GUILayout.Height(25)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void Confirm()
        {
            if (!string.IsNullOrWhiteSpace(m_keyName))
            {
                m_onConfirm?.Invoke(m_keyName);
                Close();
            }
        }
    }
}