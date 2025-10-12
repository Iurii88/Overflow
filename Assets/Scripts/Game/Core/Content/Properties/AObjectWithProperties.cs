using System;
using System.Collections.Generic;
using VContainer.Unity;
using ZLinq;

namespace Game.Core.Content.Properties
{
    public abstract class AObjectWithProperties : IInitializable
    {
        public AContentProperty[] properties;

        private Dictionary<Type, AContentProperty> m_propertyByType;

        public bool TryGetProperties<T>(out T property) where T : AContentProperty
        {
            property = null;
            if (!m_propertyByType.TryGetValue(typeof(T), out var abstractProperty))
                return false;

            property = (T)abstractProperty;
            return true;
        }

        public T GetProperty<T>() where T : AContentProperty
        {
            return (T)m_propertyByType[typeof(T)];
        }
        
        public bool HasProperty<T>() where T : AContentProperty
        {
            return m_propertyByType.ContainsKey(typeof(T));
        }

        public virtual void Initialize()
        {
            m_propertyByType = properties.AsValueEnumerable().ToDictionary(property => property.GetType(), property => property);
        }
    }
}