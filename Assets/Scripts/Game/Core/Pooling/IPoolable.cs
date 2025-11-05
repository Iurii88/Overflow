namespace Game.Core.Pooling
{
    public interface IPoolable
    {
        void OnRentedFromPool();
        void OnReturnedToPool();
    }
}
