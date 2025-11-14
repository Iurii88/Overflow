using System;
using System.Collections.Generic;

namespace Game.Core.UI.Blackboard
{
    public class EntityBlackboard
    {
        private readonly Dictionary<string, object> m_values = new();

        public event Action<string, object> OnVariableChanged;

        public void Set<T>(string key, T value)
        {
            if (m_values.TryGetValue(key, out var existingValue))
            {
                if (existingValue is T typedValue && EqualityComparer<T>.Default.Equals(typedValue, value))
                    return;
            }

            m_values[key] = value;
            OnVariableChanged?.Invoke(key, value);
        }

        public T Get<T>(string key)
        {
            if (m_values.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;

            return default;
        }

        public bool TryGet<T>(string key, out T result)
        {
            if (m_values.TryGetValue(key, out var value) && value is T typedValue)
            {
                result = typedValue;
                return true;
            }

            result = default;
            return false;
        }

        public bool Has(string key)
        {
            return m_values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            if (!m_values.Remove(key))
                return false;

            OnVariableChanged?.Invoke(key, null);
            return true;
        }

        public void Clear()
        {
            m_values.Clear();
            OnVariableChanged?.Invoke(null, null);
        }
    }
}
