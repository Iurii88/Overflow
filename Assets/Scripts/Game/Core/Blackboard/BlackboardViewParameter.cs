using System;
using UnityEngine;

namespace Game.Core.Blackboard
{
    [Serializable]
    public class BlackboardViewParameter<T>
    {
        [SerializeField]
        private string boundKey;

        private Blackboard m_blackboard;
        private T m_cachedValue;
        private bool m_hasCachedValue;

        public event Action<T> OnValueChanged;

        public void Initialize(Blackboard bb)
        {
            m_blackboard = bb;
            m_hasCachedValue = false;

            if (m_blackboard != null && !string.IsNullOrEmpty(boundKey))
            {
                m_blackboard.OnValueChanged += OnBlackboardValueChanged;
            }
        }

        private void OnBlackboardValueChanged(string key, object value)
        {
            if (key == boundKey && value is T typedValue)
            {
                m_cachedValue = typedValue;
                m_hasCachedValue = true;
                OnValueChanged?.Invoke(typedValue);
            }
        }

        public T Value
        {
            get
            {
                if (m_blackboard == null || string.IsNullOrEmpty(boundKey))
                    return default;

                if (m_hasCachedValue)
                    return m_cachedValue;

                m_cachedValue = m_blackboard.Get<T>(boundKey);
                m_hasCachedValue = true;

                return m_cachedValue;
            }
            set
            {
                if (m_blackboard == null || string.IsNullOrEmpty(boundKey))
                    return;

                m_blackboard.Set(boundKey, value);
                m_cachedValue = value;
                m_hasCachedValue = true;
            }
        }

        public void InvalidateCache()
        {
            m_hasCachedValue = false;
        }

        public Type GetValueType()
        {
            return typeof(T);
        }

        public void Dispose()
        {
            if (m_blackboard != null)
                m_blackboard.OnValueChanged -= OnBlackboardValueChanged;
        }
    }
}