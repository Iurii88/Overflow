using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.UI
{
    public class Blackboard : MonoBehaviour
    {
        [SerializeReference]
        private List<BlackboardVariable> values = new();

        private Dictionary<string, BlackboardVariable> m_cache;

        public event Action<string, BlackboardVariable> OnVariableChanged;

        private void OnEnable()
        {
            RebuildCache();
        }

        private void RebuildCache()
        {
            m_cache = new Dictionary<string, BlackboardVariable>();
            foreach (var val in values)
            {
                if (val != null && !string.IsNullOrEmpty(val.key))
                    m_cache[val.key] = val;
            }
        }

        public void Set<T>(string key, T value)
        {
            if (m_cache == null)
                RebuildCache();

            if (m_cache.TryGetValue(key, out var existing))
            {
                if (existing is BlackboardVariable<T> typedValue)
                {
                    if (!EqualityComparer<T>.Default.Equals(typedValue.value, value))
                    {
                        typedValue.value = value;
                        OnVariableChanged?.Invoke(key, typedValue);
                    }
                }
                else
                {
                    var newValue = new BlackboardVariable<T>(key, value);
                    values.Remove(existing);
                    values.Add(newValue);
                    m_cache[key] = newValue;
                    OnVariableChanged?.Invoke(key, newValue);
                }
            }
            else
            {
                var newValue = new BlackboardVariable<T>(key, value);
                values.Add(newValue);
                m_cache[key] = newValue;
                OnVariableChanged?.Invoke(key, newValue);
            }
        }

        public T Get<T>(string key)
        {
            if (m_cache == null)
                RebuildCache();

            if (m_cache.TryGetValue(key, out var value) && value is BlackboardVariable<T> typedValue)
                return typedValue.value;

            return default;
        }

        public bool TryGet<T>(string key, out T result)
        {
            if (m_cache == null)
                RebuildCache();

            if (m_cache.TryGetValue(key, out var value) && value is BlackboardVariable<T> typedValue)
            {
                result = typedValue.value;
                return true;
            }

            result = default;
            return false;
        }

        public bool Has(string key)
        {
            if (m_cache == null)
                RebuildCache();

            return m_cache.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            if (m_cache == null)
                RebuildCache();

            if (!m_cache.TryGetValue(key, out var value))
                return false;

            values.Remove(value);
            m_cache.Remove(key);
            OnVariableChanged?.Invoke(key, null);
            return true;
        }

        public void Clear()
        {
            values.Clear();
            m_cache?.Clear();
            OnVariableChanged?.Invoke(null, null);
        }

#if UNITY_EDITOR
        public void NotifyValueChangedInEditor(string key)
        {
            if (m_cache == null)
                RebuildCache();

            if (m_cache.TryGetValue(key, out var value))
            {
                OnVariableChanged?.Invoke(key, value);
            }
        }
#endif
    }
}