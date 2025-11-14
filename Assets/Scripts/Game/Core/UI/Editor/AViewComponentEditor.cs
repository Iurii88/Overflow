using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Core.UI.Blackboards;
using UnityEditor;
using UnityEngine;
using ZLinq;

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

            ButtonDrawerUtility.DrawButtonMethods(target, targets);

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
                    var blackboard = viewComponent.GetComponentInParent<BlackboardComponent>();
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
                    var blackboard = viewComponent.gameObject.AddComponent<BlackboardComponent>();
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

            var boundGuidProperty = fieldProperty.FindPropertyRelative("boundGuid");
            if (boundGuidProperty == null)
                return;

            var currentGuid = boundGuidProperty.stringValue;

            var availableVariables = GetBlackboardVariables(viewComponent.blackboard, field.FieldType);

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

            // Find current variable by GUID
            var currentVariable = availableVariables.variables.AsValueEnumerable().FirstOrDefault(v => v.Guid == currentGuid);
            var isGuidValid = currentVariable != null;

            if (availableVariables.names.Count > 0)
            {
                var currentIndex = isGuidValid ? availableVariables.variables.IndexOf(currentVariable) + 1 : 0;

                var newIndex = EditorGUILayout.Popup(currentIndex, availableVariables.names.ToArray(), GUILayout.MinWidth(80), GUILayout.MaxWidth(120));
                if (newIndex != currentIndex)
                {
                    var newGuid = newIndex == 0 ? "" : availableVariables.variables[newIndex - 1].Guid;
                    boundGuidProperty.stringValue = newGuid;

                    // Apply property changes immediately so the new GUID is set
                    serializedObject.ApplyModifiedProperties();

                    // Reinitialize the view component to subscribe to the new variable
                    ReinitializeViewComponent(viewComponent);
                }
            }
            else
            {
                EditorGUILayout.LabelField("(No variables)", EditorStyles.miniLabel, GUILayout.MinWidth(80), GUILayout.MaxWidth(100));
            }

            // Show [+] button when: no valid GUID is selected OR when there are no variables at all
            if (currentVariable == null || availableVariables.variables.Count == 0)
            {
                if (GUILayout.Button("+", GUILayout.Width(25), GUILayout.Height(18)))
                {
                    var uppercaseFieldName = field.Name.ToUpper();
                    KeyNameInputWindow.Show(uppercaseFieldName, newKeyName =>
                    {
                        if (!string.IsNullOrWhiteSpace(newKeyName))
                        {
                            CreateNewVariable(field, viewComponent, boundGuidProperty, newKeyName);
                        }
                    });
                }
            }

            GUILayout.FlexibleSpace();

            var currentValue = currentVariable != null ? GetVariableValue(currentVariable) : null;
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

        private void CreateNewVariable(FieldInfo field, AViewComponent viewComponent,
            SerializedProperty boundGuidProperty, string newKey)
        {
            if (string.IsNullOrWhiteSpace(newKey))
                return;

            var rebuildCacheMethod = typeof(BlackboardComponent).GetMethod("RebuildCache",
                BindingFlags.NonPublic | BindingFlags.Instance);
            rebuildCacheMethod?.Invoke(viewComponent.blackboard, null);

            var valueType = field.FieldType.GetGenericArguments()[0];
            var defaultValue = GetDefaultValue(valueType);

            var setMethod = typeof(BlackboardComponent).GetMethod("Set").MakeGenericMethod(valueType);
            setMethod.Invoke(viewComponent.blackboard, new[] { newKey, defaultValue });

            // Get the newly created variable's GUID
            var allVariables = viewComponent.blackboard.GetAllVariables();
            var newVariable = allVariables.AsValueEnumerable().FirstOrDefault(v => v.key == newKey);
            if (newVariable != null)
            {
                boundGuidProperty.stringValue = newVariable.Guid;
            }

            // Apply property changes immediately so the new GUID is set
            boundGuidProperty.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(viewComponent.blackboard);

            // Reinitialize the view component to subscribe to the new variable
            ReinitializeViewComponent(viewComponent);
        }

        private static void ReinitializeViewComponent(AViewComponent viewComponent)
        {
            if (viewComponent == null)
                return;

            // Disable then enable to trigger OnDisable/OnEnable which reinitializes parameters
            // Serialized properties are already applied before calling this method
            viewComponent.enabled = false;
            viewComponent.enabled = true;

            // Force the view to update immediately
            EditorApplication.QueuePlayerLoopUpdate();
            EditorUtility.SetDirty(viewComponent);
        }

        private static string GetVariableValue(BlackboardVariable variable)
        {
            if (variable == null)
                return null;

            var valueField = variable.GetType().GetField("value");
            var value = valueField?.GetValue(variable);

            if (value == null)
                return "null";

            var valueStr = value.ToString();
            if (valueStr.Length > 30)
                return valueStr.Substring(0, 27) + "...";

            return valueStr;
        }

        private static (List<string> names, List<BlackboardVariable> variables) GetBlackboardVariables(BlackboardComponent blackboard, Type parameterType)
        {
            var names = new List<string> { "(None)" };
            var variables = new List<BlackboardVariable>();
            var valueType = parameterType.GetGenericArguments()[0];

            var allVariables = blackboard.GetAllVariables();

            foreach (var variable in allVariables)
            {
                if (variable == null)
                    continue;

                var type = variable.GetValueType();
                if (type != valueType && valueType != typeof(object))
                    continue;

                variables.Add(variable);
                var displayName = string.IsNullOrEmpty(variable.key)
                    ? $"<Unnamed ({variable.Guid.Substring(0, 8)}...)>"
                    : variable.key;
                names.Add(displayName);
            }

            return (names, variables);
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