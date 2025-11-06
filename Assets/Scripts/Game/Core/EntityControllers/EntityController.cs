using UnsafeEcs.Core.Entities;

namespace Game.Core.EntityControllers
{
    public abstract class EntityController : IEntityController
    {
        public Entity Entity { get; private set; }

        public virtual void OnAwake(Entity entity)
        {
            Entity = entity;
        }

        public virtual void OnDestroy()
        {
        }
    }
}
