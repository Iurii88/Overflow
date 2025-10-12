namespace Game.Core.Pooling
{
    public interface IPoolManager
    {
        Pool<T> GetPool<T>() where T : class;
        bool TryGetPool<T>(out Pool<T> pool) where T : class;
        void RegisterPool<T>(Pool<T> pool) where T : class;
        void ClearAll();
    }
}