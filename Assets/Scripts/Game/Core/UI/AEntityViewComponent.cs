using System;
using Game.Core.UI.Blackboard;
using UnsafeEcs.Core.Entities;

namespace Game.Core.UI
{
    public abstract class AEntityViewComponent : AViewComponent
    {
        public Entity entity;

        private EntityBlackboard m_entityBlackboard;
        private Action<string, object> m_syncHandler;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (entity != default && entity.TryGetReference(out EntityBlackboard entityBlackboard))
            {
                m_entityBlackboard = entityBlackboard;
                SetupBlackboardSync();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            TeardownBlackboardSync();
        }

        private void SetupBlackboardSync()
        {
            if (m_entityBlackboard == null || blackboard == null)
                return;

            m_syncHandler = (key, value) =>
            {
                if (key == null || value == null)
                    return;

                var valueType = value.GetType();
                var setMethod = typeof(Blackboard.Blackboard).GetMethod(nameof(Blackboard.Blackboard.Set));
                var genericSetMethod = setMethod.MakeGenericMethod(valueType);
                genericSetMethod.Invoke(blackboard, new[] { key, value });
            };

            m_entityBlackboard.OnVariableChanged += m_syncHandler;
        }

        private void TeardownBlackboardSync()
        {
            if (m_entityBlackboard != null && m_syncHandler != null)
            {
                m_entityBlackboard.OnVariableChanged -= m_syncHandler;
                m_syncHandler = null;
            }

            m_entityBlackboard = null;
        }
    }
}
