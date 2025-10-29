using System;
using System.Collections.Generic;
using VContainer.Unity;
using ZLinq;

namespace Game.Core.Content.Properties
{
    public abstract class AObjectWithProperties : IInitializable
    {
        public AContentProperty[] properties;

        private Dictionary<Type, List<AContentProperty>> m_propertiesByType;
        private Dictionary<string, AContentProperty> m_propertiesByIdentifier;

        public bool TryGetProperties<T>(out T property) where T : AContentProperty
        {
            property = null;
            if (!m_propertiesByType.TryGetValue(typeof(T), out var propertyList) || propertyList.Count == 0)
                return false;

            property = (T)propertyList[0];
            return true;
        }

        public T GetProperty<T>() where T : AContentProperty
        {
            return (T)m_propertiesByType[typeof(T)][0];
        }

        public T GetProperty<T>(string identifier) where T : AContentProperty
        {
            if (m_propertiesByIdentifier.TryGetValue(identifier, out var property))
                return property as T;

            return null;
        }

        public T[] GetProperties<T>() where T : AContentProperty
        {
            if (!m_propertiesByType.TryGetValue(typeof(T), out var propertyList))
                return Array.Empty<T>();

            var result = new T[propertyList.Count];
            for (var i = 0; i < propertyList.Count; i++)
                result[i] = (T)propertyList[i];

            return result;
        }

        public bool TryGetProperty<T>(string identifier, out T property) where T : AContentProperty
        {
            property = null;
            if (!m_propertiesByIdentifier.TryGetValue(identifier, out var abstractProperty))
                return false;

            property = abstractProperty as T;
            return property != null;
        }

        public bool HasProperty<T>() where T : AContentProperty
        {
            return m_propertiesByType.ContainsKey(typeof(T)) && m_propertiesByType[typeof(T)].Count > 0;
        }

        public bool HasProperty<T>(string identifier) where T : AContentProperty
        {
            return m_propertiesByIdentifier.TryGetValue(identifier, out var property) && property is T;
        }

        public virtual void Initialize()
        {
            m_propertiesByType = new Dictionary<Type, List<AContentProperty>>();
            m_propertiesByIdentifier = new Dictionary<string, AContentProperty>();

            foreach (var property in properties.AsValueEnumerable())
            {
                var type = property.GetType();

                // Add to type dictionary
                if (!m_propertiesByType.TryGetValue(type, out var list))
                {
                    list = new List<AContentProperty>();
                    m_propertiesByType[type] = list;
                }

                list.Add(property);

                // Add to identifier dictionary if identifier is set
                if (!string.IsNullOrEmpty(property.identifier))
                    m_propertiesByIdentifier[property.identifier] = property;
            }
        }
    }
}