using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Core.Pooling
{
    public static class GameObjectPoolExtensions
    {
        public static async UniTask<GameObject> GetGameObjectAsync(this IAsyncPoolManager poolManager, string assetPath)
        {
            return await poolManager.GetAsync<GameObject>(
                key: assetPath,
                asyncFactory: async () =>
                {
                    var prefab = await poolManager.AddressableManager.LoadAssetAsync<GameObject>(assetPath);
                    return prefab;
                },
                syncClone: prefab => Object.Instantiate(prefab),
                onGet: go => go.SetActive(true),
                onRelease: go =>
                {
                    if (go != null)
                    {
                        go.SetActive(false);
                        go.transform.SetParent(null);
                    }
                }
            );
        }
    }
}
