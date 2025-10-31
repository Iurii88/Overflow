using System;
using Game.Core.Initialization;

namespace Game.Core.Content
{
    public interface IContentManager : IAsyncLoader, IDisposable
    {
        public bool IsInitialized { get; }
        T Get<T>(string id) where T : class;
        T[] GetAll<T>() where T : class;
    }
}