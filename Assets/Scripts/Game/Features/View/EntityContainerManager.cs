using System;
using Game.Core.Reflection.Attributes;
using UnityEngine;

namespace Game.Features.View
{
    [AutoRegister]
    public class EntityContainerManager : IDisposable
    {
        private Transform m_entitiesRoot;

        public Transform GetOrCreateEntityContainer()
        {
            if (m_entitiesRoot != null)
                return m_entitiesRoot;

            var rootObject = new GameObject("[Entities]");
            m_entitiesRoot = rootObject.transform;

            return m_entitiesRoot;
        }

        public void Dispose()
        {
            if (m_entitiesRoot == null)
                return;

            UnityEngine.Object.Destroy(m_entitiesRoot.gameObject);
            m_entitiesRoot = null;
        }
    }
}