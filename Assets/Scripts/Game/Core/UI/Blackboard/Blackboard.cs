using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.UI.Blackboard
{
    [ExecuteAlways]
    public class Blackboard : MonoBehaviour
    {
        [SerializeReference]
        private List<BlackboardVariable> values = new();

        private Dictionary<string, BlackboardVariable> m_keyCache;
        private Dictionary<string, BlackboardVariable> m_guidCache;

        public event Action<string, BlackboardVariable> OnVariableChanged;

        private void OnEnable()
        {
            RebuildCache();
        }

        private void RebuildCache()
        {
            m_keyCache = new Dictionary<string, BlackboardVariable>();
            m_guidCache = new Dictionary<string, BlackboardVariable>();

            foreach (var val in values)
            {
                if (val == null)
                    continue;

                // Cache by GUID (primary identifier)
                var guid = val.Guid;
                if (!string.IsNullOrEmpty(guid))
                    m_guidCache[guid] = val;

                // Cache by key (for debugging and backward compatibility)
                if (!string.IsNullOrEmpty(val.key))
                    m_keyCache[val.key] = val;
            }
        }

        /// <summary>
        ///     Gets a variable by its GUID.
        /// </summary>
        public BlackboardVariable GetVariableByGuid(string guid)
        {
            if (m_guidCache == null)
                RebuildCache();

            m_guidCache.TryGetValue(guid, out var variable);
            return variable;
        }

        /// <summary>
        ///     Gets all variables for editor display.
        /// </summary>
        public List<BlackboardVariable> GetAllVariables()
        {
            return values;
        }

        public void Set<T>(string key, T value)
        {
            if (m_keyCache == null)
                RebuildCache();

            if (m_keyCache.TryGetValue(key, out var existing))
            {
                if (existing is BlackboardVariable<T> typedValue)
                {
                    if (EqualityComparer<T>.Default.Equals(typedValue.value, value))
                        return;

                    typedValue.value = value;
                    OnVariableChanged?.Invoke(existing.Guid, typedValue);
                }
                else
                {
                    var newValue = new BlackboardVariable<T>(key, value);
                    values.Remove(existing);
                    values.Add(newValue);
                    m_keyCache[key] = newValue;
                    m_guidCache[newValue.Guid] = newValue;
                    OnVariableChanged?.Invoke(newValue.Guid, newValue);
                }
            }
            else
            {
                var newValue = new BlackboardVariable<T>(key, value);
                values.Add(newValue);
                m_keyCache[key] = newValue;
                m_guidCache[newValue.Guid] = newValue;
                OnVariableChanged?.Invoke(newValue.Guid, newValue);
            }
        }

        public T Get<T>(string key)
        {
            if (m_keyCache == null)
                RebuildCache();

            if (m_keyCache.TryGetValue(key, out var value) && value is BlackboardVariable<T> typedValue)
                return typedValue.value;

            return default;
        }

        public T GetByGuid<T>(string guid)
        {
            if (m_guidCache == null)
                RebuildCache();

            if (m_guidCache.TryGetValue(guid, out var value) && value is BlackboardVariable<T> typedValue)
                return typedValue.value;

            return default;
        }

        public bool TryGet<T>(string key, out T result)
        {
            if (m_keyCache == null)
                RebuildCache();

            if (m_keyCache.TryGetValue(key, out var value) && value is BlackboardVariable<T> typedValue)
            {
                result = typedValue.value;
                return true;
            }

            result = default;
            return false;
        }

        public bool TryGetByGuid<T>(string guid, out T result)
        {
            if (m_guidCache == null)
                RebuildCache();

            if (m_guidCache.TryGetValue(guid, out var value) && value is BlackboardVariable<T> typedValue)
            {
                result = typedValue.value;
                return true;
            }

            result = default;
            return false;
        }

        public bool Has(string key)
        {
            if (m_keyCache == null)
                RebuildCache();

            return m_keyCache.ContainsKey(key);
        }

        public bool HasGuid(string guid)
        {
            if (m_guidCache == null)
                RebuildCache();

            return m_guidCache.ContainsKey(guid);
        }

        public bool Remove(string key)
        {
            if (m_keyCache == null)
                RebuildCache();

            if (!m_keyCache.TryGetValue(key, out var value))
                return false;

            values.Remove(value);
            m_keyCache.Remove(key);
            m_guidCache.Remove(value.Guid);
            OnVariableChanged?.Invoke(value.Guid, null);
            return true;
        }

        public bool RemoveByGuid(string guid)
        {
            if (m_guidCache == null)
                RebuildCache();

            if (!m_guidCache.TryGetValue(guid, out var value))
                return false;

            values.Remove(value);
            m_guidCache.Remove(guid);
            if (!string.IsNullOrEmpty(value.key))
                m_keyCache.Remove(value.key);
            OnVariableChanged?.Invoke(guid, null);
            return true;
        }

        public void Clear()
        {
            values.Clear();
            m_keyCache?.Clear();
            m_guidCache?.Clear();
            OnVariableChanged?.Invoke(null, null);
        }

        /// <summary>
        ///     Notifies listeners that a variable has changed.
        /// </summary>
        public void NotifyVariableChanged(string guid, BlackboardVariable variable)
        {
            OnVariableChanged?.Invoke(guid, variable);
        }

#if UNITY_EDITOR
        public void NotifyValueChangedInEditor(string guid)
        {
            if (m_guidCache == null)
                RebuildCache();

            if (m_guidCache.TryGetValue(guid, out var value))
            {
                OnVariableChanged?.Invoke(guid, value);
            }
        }
#endif
    }
}