using UnityEditor;
using UnityEngine;

namespace Game.Core.UI.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class BaseMonoBehaviourEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ButtonDrawerUtility.DrawButtonMethods(target, targets);
        }
    }
}
