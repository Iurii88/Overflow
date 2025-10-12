using System;
using System.Collections.Generic;
using Game.Core.Reflection.Attributes;

namespace Game.Core.Pooling
{
    [AutoRegister]
    public class PoolManager : IPoolManager
    {
        private readonly Dictionary<Type, object> m_pools = new();

        public Pool<T> GetPool<T>() where T : class
        {
            var type = typeof(T);

            if (m_pools.TryGetValue(type, out var pool))
                return (Pool<T>)pool;

            throw new InvalidOperationException($"Pool for type {type.Name} is not registered");
        }

        public bool TryGetPool<T>(out Pool<T> pool) where T : class
        {
            var type = typeof(T);

            if (m_pools.TryGetValue(type, out var poolObj))
            {
                pool = (Pool<T>)poolObj;
                return true;
            }

            pool = null;
            return false;
        }

        public void RegisterPool<T>(Pool<T> pool) where T : class
        {
            var type = typeof(T);
            if (!m_pools.TryAdd(type, pool))
                throw new InvalidOperationException($"Pool for type {type.Name} is already registered");
        }

        public void ClearAll()
        {
            foreach (var pool in m_pools.Values)
                if (pool is IDisposable disposable)
                    disposable.Dispose();

            m_pools.Clear();
        }
    }
}