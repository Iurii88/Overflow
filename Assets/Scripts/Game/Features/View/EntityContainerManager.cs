using Game.Features.Sessions.Attributes;
using UnityEngine;

namespace Game.Features.View
{
    [AutoRegister]
    public class EntityContainerManager : IEntityContainerManager
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

            Object.Destroy(m_entitiesRoot.gameObject);
            m_entitiesRoot = null;
        }
    }
}