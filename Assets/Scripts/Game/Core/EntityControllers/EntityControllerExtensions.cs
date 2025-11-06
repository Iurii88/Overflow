using Game.Core.Logging;
using UnsafeEcs.Core.Entities;

namespace Game.Core.EntityControllers
{
    public static class EntityControllerExtensions
    {
        public static T AddController<T>(this Entity entity) where T : class, IEntityController, new()
        {
            if (HasController<T>(entity))
            {
                GameLogger.Error($"Entity {entity.id} already has controller of type {typeof(T).Name}");
                return entity.GetReference<T>();
            }

            var controller = new T();
            entity.AddReference(controller);
            controller.OnAwake(entity);
            return controller;
        }

        public static T GetController<T>(this Entity entity) where T : class, IEntityController
        {
            return entity.GetReference<T>();
        }

        public static T GetOrCreateController<T>(this Entity entity) where T : class, IEntityController, new()
        {
            if (entity.TryGetReference(out T controller))
                return controller;

            return AddController<T>(entity);
        }

        public static bool HasController<T>(this Entity entity) where T : class, IEntityController
        {
            return entity.HasReference<T>();
        }

        public static bool RemoveController<T>(this Entity entity) where T : class, IEntityController
        {
            if (!entity.TryGetReference(out T controller))
                return false;

            controller.OnDestroy();
            entity.RemoveReference<T>();
            return true;
        }
    }
}