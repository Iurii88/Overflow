using System;
using Game.Core.Bootstraps.Interfaces;

namespace Game.Core.Content
{
    public interface IContentManager : IAsyncLoader, IDisposable
    {
        public bool isInitialized { get; }
        T Get<T>(string id) where T : class;
        T[] GetAll<T>() where T : class;
    }
}