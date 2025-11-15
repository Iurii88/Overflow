using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Core.Pooling
{
    public interface IAsyncPoolManager : IDisposable
    {
        UniTask<T> GetAsync<T>(string key, Func<UniTask<T>> asyncFactory, Func<T, T> syncClone, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null) where T : class;
        UniTask<GameObject> GetGameObjectAsync(string assetPath, bool activeByDefault = true);
        void Release<T>(T obj) where T : class;
        GameObject GetPoolRoot<T>(string key) where T : class;
    }
}