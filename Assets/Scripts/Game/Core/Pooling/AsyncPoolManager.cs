using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Reflection.Attributes;
using VContainer;

namespace Game.Core.Pooling
{
    [AutoRegister]
    public class AsyncPoolManager : IAsyncPoolManager
    {
        private class PoolData<T> where T : class
        {
            public Pool<T> pool;
            public readonly Dictionary<T, PoolHandle> handles = new();
        }

        [Inject]
        private IAddressableManager m_addressableManager;

        private readonly Dictionary<(Type, string), object> m_pools = new();

        public IAddressableManager AddressableManager => m_addressableManager;

        public async UniTask<T> GetAsync<T>(string key, Func<UniTask<T>> asyncFactory, Func<T, T> syncClone, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null) where T : class
        {
            var poolKey = (typeof(T), key);

            if (!m_pools.TryGetValue(poolKey, out var poolObj))
            {
                var template = await asyncFactory();

                var pool = new Pool<T>(
                    () => syncClone(template),
                    onGet,
                    onRelease,
                    onDestroy
                );

                poolObj = new PoolData<T> { pool = pool };
                m_pools[poolKey] = poolObj;
            }

            var data = (PoolData<T>)poolObj;
            var handle = data.pool.Rent(out var obj);
            data.handles[obj] = handle;

            return obj;
        }

        public void Release<T>(T obj) where T : class
        {
            foreach (var kvp in m_pools)
            {
                if (kvp.Key.Item1 != typeof(T) || kvp.Value is not PoolData<T> poolData)
                    continue;

                if (!poolData.handles.TryGetValue(obj, out var handle))
                    continue;

                if (handle.index == -1)
                {
                    poolData.handles.Remove(obj);
                    return;
                }

                poolData.pool.Return(handle);
                poolData.handles.Remove(obj);
                return;
            }
        }
    }
}