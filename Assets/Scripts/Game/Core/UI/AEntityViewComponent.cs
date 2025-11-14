using System;
using Game.Core.UI.Blackboards;
using UnsafeEcs.Core.Entities;

namespace Game.Core.UI
{
    public abstract class AEntityViewComponent : AViewComponent
    {
        public Entity entity;

        private Blackboard m_entityBlackboard;
        private Action<string, object> m_syncHandler;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (entity.IsAlive() && entity.TryGetReference(out Blackboard entityBlackboard) && blackboard != null)
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

                // Call Set using reflection to handle generic types at runtime
                var valueType = value.GetType();
                var setMethod = typeof(BlackboardComponent).GetMethod(nameof(BlackboardComponent.Set));
                var genericSetMethod = setMethod.MakeGenericMethod(valueType);
                genericSetMethod.Invoke(blackboard, new[] { key, value });
            };

            m_entityBlackboard.OnVariableChanged += m_syncHandler;

            // Sync all existing values
            foreach (var kvp in m_entityBlackboard.GetAll())
            {
                if (kvp.Key != null && kvp.Value != null)
                {
                    var valueType = kvp.Value.GetType();
                    var setMethod = typeof(BlackboardComponent).GetMethod(nameof(BlackboardComponent.Set));
                    var genericSetMethod = setMethod.MakeGenericMethod(valueType);
                    genericSetMethod.Invoke(blackboard, new[] { kvp.Key, kvp.Value });
                }
            }
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