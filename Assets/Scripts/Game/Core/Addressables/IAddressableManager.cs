using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Game.Core.Addressables
{
    public interface IAddressableManager : IDisposable
    {
        UniTask<T> LoadAssetAsync<T>(string key, CancellationToken ct = default) where T : Object;
        UniTask<IList<T>> LoadAssetsAsync<T>(IEnumerable<string> keys, CancellationToken ct = default) where T : Object;
        void Release(string key);
        void ReleaseAll();
    }
}