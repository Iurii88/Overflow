using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Blackboard
{
    public class Blackboard : MonoBehaviour
    {
        [SerializeReference]
        private List<BlackboardValue> values = new();

        private Dictionary<string, BlackboardValue> m_cache;
        private readonly Dictionary<string, object> m_previousValues = new();

        public event Action<string, object> OnValueChanged;

        private void OnEnable()
        {
            RebuildCache();
            CaptureCurrentValues();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_cache == null)
                RebuildCache();

            CheckForChanges();
        }
#endif

        private void CaptureCurrentValues()
        {
            m_previousValues.Clear();
            if (m_cache == null)
                return;

            foreach (var kvp in m_cache)
                m_previousValues[kvp.Key] = kvp.Value.GetObjectValue();
        }

        private void CheckForChanges()
        {
            if (m_cache == null)
                return;

            foreach (var kvp in m_cache)
            {
                var currentValue = kvp.Value.GetObjectValue();

                if (m_previousValues.TryGetValue(kvp.Key, out var previousValue))
                {
                    if (Equals(currentValue, previousValue))
                        continue;
                    
                    OnValueChanged?.Invoke(kvp.Key, currentValue);
                    m_previousValues[kvp.Key] = currentValue;
                }
                else
                    OnValueChanged?.Invoke(kvp.Key, currentValue);
            }
        }

        private void RebuildCache()
        {
            m_cache = new Dictionary<string, BlackboardValue>();
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
                if (existing is BlackboardValue<T> typedValue)
                {
                    typedValue.value = value;
                    m_previousValues[key] = value;
                    OnValueChanged?.Invoke(key, value);
                }
                else
                {
                    var newValue = new BlackboardValue<T>(key, value);
                    values.Remove(existing);
                    values.Add(newValue);
                    m_cache[key] = newValue;
                    m_previousValues[key] = value;
                    OnValueChanged?.Invoke(key, value);
                }
            }
            else
            {
                var newValue = new BlackboardValue<T>(key, value);
                values.Add(newValue);
                m_cache[key] = newValue;
                m_previousValues[key] = value;
                OnValueChanged?.Invoke(key, value);
            }
        }

        public T Get<T>(string key)
        {
            if (m_cache == null)
                RebuildCache();

            if (m_cache.TryGetValue(key, out var value) && value is BlackboardValue<T> typedValue)
                return typedValue.value;

            return default;
        }

        public bool TryGet<T>(string key, out T result)
        {
            if (m_cache == null)
                RebuildCache();

            if (m_cache.TryGetValue(key, out var value) && value is BlackboardValue<T> typedValue)
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
            m_previousValues.Remove(key);
            OnValueChanged?.Invoke(key, null);
            return true;
        }

        public void Clear()
        {
            values.Clear();
            m_cache?.Clear();
            m_previousValues.Clear();
            OnValueChanged?.Invoke(null, null);
        }
    }
}