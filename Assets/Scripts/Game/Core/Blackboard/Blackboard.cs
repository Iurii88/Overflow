using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Blackboard
{
    [Serializable]
    public abstract class BlackboardValue
    {
        public string key;
        public abstract Type GetValueType();
        public abstract object GetObjectValue();
    }

    [Serializable]
    public class BlackboardValue<T> : BlackboardValue
    {
        public T value;

        public BlackboardValue()
        {
        }

        public BlackboardValue(string key, T value)
        {
            this.key = key;
            this.value = value;
        }

        public override Type GetValueType()
        {
            return typeof(T);
        }

        public override object GetObjectValue()
        {
            return value;
        }
    }

    public class Blackboard : MonoBehaviour
    {
        [SerializeReference]
        private List<BlackboardValue> values = new();

        private Dictionary<string, BlackboardValue> m_cache;

        private void OnEnable()
        {
            RebuildCache();
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
                    typedValue.value = value;
                else
                {
                    var newValue = new BlackboardValue<T>(key, value);
                    values.Remove(existing);
                    values.Add(newValue);
                    m_cache[key] = newValue;
                }
            }
            else
            {
                var newValue = new BlackboardValue<T>(key, value);
                values.Add(newValue);
                m_cache[key] = newValue;
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

            if (m_cache.TryGetValue(key, out var value))
            {
                values.Remove(value);
                m_cache.Remove(key);
                return true;
            }

            return false;
        }

        public void Clear()
        {
            values.Clear();
            m_cache?.Clear();
        }
    }
}