using Game.Core.Content.Attributes;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    [CustomPropertyDrawer(typeof(ContentSelectorAttribute))]
    public class ContentIdAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "ContentId attribute can only be used on string fields");
                return;
            }

            var contentIdAttribute = attribute as ContentSelectorAttribute;
            if (contentIdAttribute == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            const float buttonWidth = 60f;
            const float spacing = 2f;
            var popupRect = new Rect(position.x, position.y, position.width - buttonWidth - spacing, position.height);
            var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, position.height);

            var contentIds = ContentIdUtility.GetContentIds(contentIdAttribute.contentType);

            var displayOptions = new string[contentIds.Length + 1];
            displayOptions[0] = "(None)";
            System.Array.Copy(contentIds, 0, displayOptions, 1, contentIds.Length);

            var currentValue = property.stringValue;
            int currentIndex;

            if (string.IsNullOrEmpty(currentValue))
            {
                currentIndex = 0;
            }
            else
            {
                var valueIndex = System.Array.IndexOf(contentIds, currentValue);
                currentIndex = valueIndex >= 0 ? valueIndex + 1 : 0;
            }

            var newIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, displayOptions);
            if (newIndex != currentIndex)
            {
                if (newIndex == 0)
                {
                    property.stringValue = string.Empty;
                }
                else if (newIndex > 0 && newIndex <= contentIds.Length)
                {
                    property.stringValue = contentIds[newIndex - 1];
                }
            }

            if (GUI.Button(buttonRect, "Refresh"))
            {
                ContentIdUtility.ClearCache();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}