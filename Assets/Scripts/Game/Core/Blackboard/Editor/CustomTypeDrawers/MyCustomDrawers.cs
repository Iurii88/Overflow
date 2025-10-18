using UnityEditor;
using UnityEngine;

namespace Game.Core.Blackboard.Editor.CustomDrawers
{
    [InitializeOnLoad]
    public static class MyCustomDrawers
    {
        static MyCustomDrawers()
        {
            CustomBlackboardTypeDrawers.Register<System.Numerics.Vector3>((rect, label, value, _) =>
            {
                var v = value != null ? (System.Numerics.Vector3)value : System.Numerics.Vector3.Zero;

                var unityVec = new Vector3(v.X, v.Y, v.Z);
                var newUnityVec = EditorGUI.Vector3Field(rect, label, unityVec);

                return new System.Numerics.Vector3(newUnityVec.x, newUnityVec.y, newUnityVec.z);
            });
        }
    }
}
