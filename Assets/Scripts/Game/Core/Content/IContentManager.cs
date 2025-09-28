using System;
using VContainer.Unity;

namespace Game.Core.Content
{
    public interface IContentManager : IAsyncStartable, IDisposable
    {
        T Get<T>(string id) where T : class;
        T[] GetAll<T>() where T : class;
    }
}