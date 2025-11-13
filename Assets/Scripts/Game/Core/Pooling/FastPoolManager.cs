using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Reflection.Attributes;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Game.Core.Pooling
{
    [AutoRegister]
    public class FastPoolManager : IAsyncPoolManager
    {
        [Inject]
        private IObjectResolver m_resolver;

        private sealed class FastPool<T> : PoolBase where T : class
        {
            private T[] m_items;
            private int m_count;
            private readonly T m_template;
            private readonly Func<T, T> m_cloneFunc;
            private readonly Action<T> m_onGet;
            private readonly Action<T> m_onRelease;

            public FastPool(T template, Func<T, T> cloneFunc, Action<T> onGet, Action<T> onRelease, int initialCapacity = 32)
            {
                m_template = template;
                m_cloneFunc = cloneFunc;
                m_onGet = onGet;
                m_onRelease = onRelease;
                m_items = new T[initialCapacity];
                m_count = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Get()
            {
                T item;

                if (m_count > 0)
                {
                    m_count--;
                    item = m_items[m_count];
                    m_items[m_count] = null;
                }
                else
                {
                    item = m_cloneFunc(m_template);
                }

                m_onGet?.Invoke(item);
                return item;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Release(T item)
            {
                m_onRelease?.Invoke(item);

                if (m_count == m_items.Length)
                {
                    var newArray = new T[m_items.Length * 2];
                    Array.Copy(m_items, newArray, m_items.Length);
                    m_items = newArray;
                }

                m_items[m_count] = item;
                m_count++;
            }
        }

        [Inject]
        private IAddressableManager m_addressableManager;

        private readonly Dictionary<PoolKey, PoolBase> m_pools = new();
        private readonly Dictionary<int, PoolKey> m_objectToPoolKey = new();
        private GameObject m_rootPoolObject;

        public IAddressableManager AddressableManager => m_addressableManager;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GameObject GetOrCreateRootPool()
        {
            if (m_rootPoolObject != null)
                return m_rootPoolObject;

            m_rootPoolObject = new GameObject("[FastPools]");
            Object.DontDestroyOnLoad(m_rootPoolObject);

            return m_rootPoolObject;
        }

        private GameObject GetOrCreatePoolRoot(string poolKey)
        {
            var root = GetOrCreateRootPool();
            var cleanNameSpan = poolKey.AsSpan();
            var lastSlash = cleanNameSpan.LastIndexOf('/');
            if (lastSlash >= 0)
            {
                cleanNameSpan = cleanNameSpan.Slice(lastSlash + 1);
            }

            var lastDot = cleanNameSpan.LastIndexOf('.');
            if (lastDot >= 0)
            {
                cleanNameSpan = cleanNameSpan.Slice(0, lastDot);
            }

            var cleanName = cleanNameSpan.ToString();
            if (string.IsNullOrEmpty(cleanName))
            {
                cleanName = poolKey;
            }

            var transform = root.transform;
            var childCount = transform.childCount;

            for (var i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == cleanName)
                {
                    return child.gameObject;
                }
            }

            var poolRoot = new GameObject(cleanName);
            poolRoot.transform.SetParent(transform, false);
            poolRoot.AddComponent<PoolRootBehaviour>();

            return poolRoot;
        }

        public async UniTask<T> GetAsync<T>(string key, Func<UniTask<T>> asyncFactory, Func<T, T> syncClone,
            Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null) where T : class
        {
            var poolKey = new PoolKey(typeof(T), key);

            if (!m_pools.TryGetValue(poolKey, out var poolBase))
            {
                var template = await asyncFactory();
                GameObject poolRoot = null;

                if (typeof(T) == typeof(GameObject))
                {
                    poolRoot = GetOrCreatePoolRoot(key);
                }

                var pool = new FastPool<T>(template, syncClone, onGet, onRelease)
                {
                    poolRoot = poolRoot
                };

                m_pools[poolKey] = pool;
                poolBase = pool;
            }

            var fastPool = (FastPool<T>)poolBase;
            var obj = fastPool.Get();

            m_objectToPoolKey[obj.GetHashCode()] = poolKey;

            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObject GetPoolRoot<T>(string key) where T : class
        {
            var poolKey = new PoolKey(typeof(T), key);
            return m_pools.TryGetValue(poolKey, out var poolBase) ? poolBase.poolRoot : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release<T>(T obj) where T : class
        {
            if (obj == null)
            {
                return;
            }

            var objHash = obj.GetHashCode();

            if (!m_objectToPoolKey.TryGetValue(objHash, out var poolKey))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Failed to release object of type {typeof(T).Name} - not found in any pool");
#endif
                return;
            }

            if (!m_pools.TryGetValue(poolKey, out var poolBase))
            {
#if UNITY_EDITOR
                Debug.LogWarning("Failed to release object - pool not found");
#endif
                return;
            }

            m_objectToPoolKey.Remove(objHash);
            ((FastPool<T>)poolBase).Release(obj);
        }

        public async UniTask<GameObject> GetGameObjectAsync(string assetPath)
        {
            var gameObject = await GetAsync(
                assetPath,
                async () =>
                {
                    var prefab = await AddressableManager.LoadAssetAsync<GameObject>(assetPath);
                    return prefab;
                },
                Object.Instantiate,
                go =>
                {
                    if (go == null)
                        return;

                    go.transform.SetParent(null);
                    ResetTransform(go.transform);

                    var poolables = go.GetComponentsInChildren<IPoolable>(true);
                    foreach (var poolable in poolables)
                        poolable.OnRentedFromPool();

                    go.SetActive(true);
                },
                go =>
                {
                    if (go == null)
                        return;

                    go.SetActive(false);

                    var poolables = go.GetComponentsInChildren<IPoolable>(true);
                    foreach (var poolable in poolables)
                        poolable.OnReturnedToPool();

                    var poolRoot = GetPoolRoot<GameObject>(assetPath);
                    go.transform.SetParent(poolRoot?.transform);
                }
            );
            m_resolver.InjectGameObject(gameObject);
            return gameObject;
        }

        private static void ResetTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public void Dispose()
        {
            m_pools.Clear();
            m_objectToPoolKey.Clear();

            if (m_rootPoolObject != null)
            {
                Object.Destroy(m_rootPoolObject);
                m_rootPoolObject = null;
            }
        }
    }
}