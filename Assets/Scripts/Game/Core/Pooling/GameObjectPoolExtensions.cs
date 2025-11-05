using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Core.Pooling
{
    public static class GameObjectPoolExtensions
    {
        public static async UniTask<GameObject> GetGameObjectAsync(this IAsyncPoolManager poolManager, string assetPath)
        {
            var gameObject = await poolManager.GetAsync(
                assetPath,
                async () =>
                {
                    var prefab = await poolManager.AddressableManager.LoadAssetAsync<GameObject>(assetPath);
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

                    var poolRoot = poolManager.GetPoolRoot<GameObject>(assetPath);
                    go.transform.SetParent(poolRoot?.transform);
                }
            );

            return gameObject;
        }

        private static void ResetTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}