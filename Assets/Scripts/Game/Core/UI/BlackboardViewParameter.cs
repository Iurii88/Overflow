using System;
using UnityEngine;

namespace Game.Core.UI
{
    [Serializable]
    public class BlackboardViewParameter<T>
    {
        [SerializeField]
        private string boundGuid;

        public string BoundGuid
        {
            get => boundGuid;
            set => boundGuid = value;
        }

        private Blackboard m_blackboard;

        public event Action<BlackboardVariable<T>> OnVariableChanged;

        public void Initialize(Blackboard bb)
        {
            m_blackboard = bb;

            if (m_blackboard != null && !string.IsNullOrEmpty(boundGuid))
                m_blackboard.OnVariableChanged += OnBlackboardVariableChanged;
        }

        private void OnBlackboardVariableChanged(string guid, BlackboardVariable variable)
        {
            if (guid == boundGuid && variable is BlackboardVariable<T> typedValue)
                OnVariableChanged?.Invoke(typedValue);
        }

        public T Value
        {
            get
            {
                if (m_blackboard == null || string.IsNullOrEmpty(boundGuid))
                    return default;

                return m_blackboard.GetByGuid<T>(boundGuid);
            }
            set
            {
                if (m_blackboard == null || string.IsNullOrEmpty(boundGuid))
                    return;

                // Find the variable by GUID and update it
                var variable = m_blackboard.GetVariableByGuid(boundGuid);
                if (variable is BlackboardVariable<T> typedVar)
                {
                    typedVar.value = value;
                    m_blackboard.NotifyVariableChanged(boundGuid, typedVar);
                }
            }
        }

        public Type GetValueType()
        {
            return typeof(T);
        }

        public void Dispose()
        {
            if (m_blackboard != null)
                m_blackboard.OnVariableChanged -= OnBlackboardVariableChanged;

            // Clear all event subscriptions to prevent memory leaks
            OnVariableChanged = null;
        }
    }
}