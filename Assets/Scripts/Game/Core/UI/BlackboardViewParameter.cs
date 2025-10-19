using System;
using UnityEngine;

namespace Game.Core.ViewComponents
{
    [Serializable]
    public class BlackboardViewParameter<T>
    {
        [SerializeField]
        private string boundKey;

        public string BoundKey
        {
            get => boundKey;
            set => boundKey = value;
        }

        private Blackboard m_blackboard;

        public event Action<BlackboardVariable<T>> OnVariableChanged;

        public void Initialize(Blackboard bb)
        {
            m_blackboard = bb;

            if (m_blackboard != null && !string.IsNullOrEmpty(boundKey))
                m_blackboard.OnVariableChanged += OnBlackboardVariableChanged;
        }

        private void OnBlackboardVariableChanged(string key, BlackboardVariable variable)
        {
            if (key == boundKey && variable is BlackboardVariable<T> typedValue)
                OnVariableChanged?.Invoke(typedValue);
        }

        public T Value
        {
            get
            {
                if (m_blackboard == null || string.IsNullOrEmpty(boundKey))
                    return default;

                return m_blackboard.Get<T>(boundKey);
            }
            set
            {
                if (m_blackboard == null || string.IsNullOrEmpty(boundKey))
                    return;

                m_blackboard.Set(boundKey, value);
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
        }
    }
}