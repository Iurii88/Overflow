using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Reflection.Attributes;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Game.Core.Pooling
{
    [AutoRegister]
    public class AsyncPoolManager : IAsyncPoolManager
    {
        private class PoolData<T> where T : class
        {
            public Pool<T> pool;
            public readonly Dictionary<T, PoolHandle> handles = new();
            public GameObject poolRoot;
        }

        [Inject]
        private IAddressableManager m_addressableManager;

        private readonly Dictionary<(Type, string), object> m_pools = new();
        private GameObject m_rootPoolObject;

        public IAddressableManager AddressableManager => m_addressableManager;

        private GameObject GetOrCreateRootPool()
        {
            if (m_rootPoolObject != null)
                return m_rootPoolObject;

            m_rootPoolObject = new GameObject("[Pools]");
            Object.DontDestroyOnLoad(m_rootPoolObject);

            return m_rootPoolObject;
        }

        private GameObject GetOrCreatePoolRoot(string poolKey)
        {
            var root = GetOrCreateRootPool();
            var cleanName = System.IO.Path.GetFileNameWithoutExtension(poolKey);
            if (string.IsNullOrEmpty(cleanName))
                cleanName = poolKey;

            var poolRoot = root.transform.Find(cleanName)?.gameObject;

            if (poolRoot != null)
                return poolRoot;

            poolRoot = new GameObject(cleanName);
            poolRoot.transform.SetParent(root.transform);
            poolRoot.AddComponent<PoolRootBehaviour>();

            return poolRoot;
        }

        public async UniTask<T> GetAsync<T>(string key, Func<UniTask<T>> asyncFactory, Func<T, T> syncClone, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null)
            where T : class
        {
            var poolKey = (typeof(T), key);

            if (!m_pools.TryGetValue(poolKey, out var poolObj))
            {
                var template = await asyncFactory();

                var poolRoot = typeof(T) == typeof(GameObject) ? GetOrCreatePoolRoot(key) : null;

                var pool = new Pool<T>(
                    () => syncClone(template),
                    onGet,
                    onRelease,
                    onDestroy
                );

                poolObj = new PoolData<T> { pool = pool, poolRoot = poolRoot };
                m_pools[poolKey] = poolObj;
            }

            var data = (PoolData<T>)poolObj;
            var handle = data.pool.Rent(out var obj);
            data.handles[obj] = handle;

            return obj;
        }

        public GameObject GetPoolRoot<T>(string key) where T : class
        {
            var poolKey = (typeof(T), key);
            if (m_pools.TryGetValue(poolKey, out var poolObj) && poolObj is PoolData<T> data)
            {
                return data.poolRoot;
            }

            return null;
        }

        public void Release<T>(T obj) where T : class
        {
            if (obj == null)
                return;

            foreach (var kvp in m_pools)
            {
                if (kvp.Key.Item1 != typeof(T) || kvp.Value is not PoolData<T> poolData)
                    continue;

                if (!poolData.handles.TryGetValue(obj, out var handle))
                    continue;

                poolData.pool.Return(handle);
                poolData.handles.Remove(obj);
                return;
            }

#if UNITY_EDITOR
            Debug.LogWarning($"Failed to release object of type {typeof(T).Name} - not found in any pool");
#endif
        }
    }
}