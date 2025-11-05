using UnityEngine;

namespace Game.Core.Pooling
{
    public class PoolRootBehaviour : MonoBehaviour
    {
#if UNITY_EDITOR
        private string m_baseName;
        private int m_lastChildCount = -1;

        private void Awake()
        {
            m_baseName = gameObject.name;
        }

        private void LateUpdate()
        {
            var childCount = transform.childCount;
            if (childCount != m_lastChildCount)
            {
                m_lastChildCount = childCount;
                gameObject.name = $"{m_baseName} [{childCount}]";
            }
        }
#endif
    }
}
