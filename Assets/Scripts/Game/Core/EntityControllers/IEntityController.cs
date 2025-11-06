using UnsafeEcs.Core.Entities;

namespace Game.Core.EntityControllers
{
    public interface IEntityController
    {
        Entity Entity { get; }
        void OnAwake(Entity entity);
        void OnDestroy();
    }
}
