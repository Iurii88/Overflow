using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Game.Core.Addressables
{
    public class AddressableManager : IAddressableManager
    {
        private readonly Dictionary<string, AsyncOperationHandle> m_loadedHandles = new();
        private readonly Dictionary<string, object> m_cachedAssets = new();

        public async UniTask<T> LoadAssetAsync<T>(string key, CancellationToken ct = default) where T : Object
        {
            if (m_cachedAssets.TryGetValue(key, out var cached))
                return cached as T;

            if (m_loadedHandles.TryGetValue(key, out var handle))
            {
                await handle.ToUniTask(cancellationToken: ct);
                return handle.Result as T;
            }

            var loadHandle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key);
            m_loadedHandles[key] = loadHandle;

            var asset = await loadHandle.ToUniTask(cancellationToken: ct);
            m_cachedAssets[key] = asset;

            return asset;
        }

        public async UniTask<IList<T>> LoadAssetsAsync<T>(IEnumerable<string> keys, CancellationToken ct = default) where T : Object
        {
            var tasks = new List<UniTask<T>>();
            foreach (var key in keys)
                tasks.Add(LoadAssetAsync<T>(key, ct));

            return await UniTask.WhenAll(tasks);
        }

        public void Release(string key)
        {
            if (!m_loadedHandles.TryGetValue(key, out var handle))
                return;

            UnityEngine.AddressableAssets.Addressables.Release(handle);
            m_loadedHandles.Remove(key);
            m_cachedAssets.Remove(key);
        }

        public void ReleaseAll()
        {
            foreach (var handle in m_loadedHandles.Values)
                UnityEngine.AddressableAssets.Addressables.Release(handle);

            m_loadedHandles.Clear();
            m_cachedAssets.Clear();
        }

        public void Dispose()
        {
            ReleaseAll();
        }
    }
}