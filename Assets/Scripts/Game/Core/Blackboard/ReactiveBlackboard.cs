using System;
using System.Collections.Generic;
using Game.Core.Logging;
using R3;
using UnityEngine;

namespace Game.Core.Blackboard
{
    public class ReactiveBlackboard : MonoBehaviour
    {
        private readonly Dictionary<string, object> m_properties = new();

        public ReactiveProperty<T> GetOrCreateProperty<T>(string key, T defaultValue = default)
        {
            if (m_properties.TryGetValue(key, out var existing))
                return existing as ReactiveProperty<T>;

            var newProperty = new ReactiveProperty<T>(defaultValue);
            m_properties[key] = newProperty;
            return newProperty;
        }

        public Observable<T> GetObservable<T>(string key)
        {
            if (m_properties.TryGetValue(key, out var property))
            {
                if (property is ReactiveProperty<T> reactiveProperty)
                    return reactiveProperty;
            }

            GameLogger.Warning($"Property '{key}' not found or wrong type in Blackboard");
            return Observable.Return(default(T));
        }

        public void SetValue<T>(string key, T value)
        {
            if (m_properties.TryGetValue(key, out var property))
            {
                if (property is ReactiveProperty<T> reactiveProperty)
                    reactiveProperty.Value = value;
            }
        }

        public T GetValue<T>(string key)
        {
            if (m_properties.TryGetValue(key, out var property))
            {
                if (property is ReactiveProperty<T> reactiveProperty)
                    return reactiveProperty.CurrentValue;
            }

            return default;
        }

        public bool HasProperty(string key) => m_properties.ContainsKey(key);

        public string[] GetAllKeys()
        {
            var keys = new string[m_properties.Count];
            m_properties.Keys.CopyTo(keys, 0);
            return keys;
        }

        private void OnDestroy()
        {
            foreach (var prop in m_properties.Values)
            {
                if (prop is IDisposable disposable)
                    disposable.Dispose();
            }

            m_properties.Clear();
        }
    }
}