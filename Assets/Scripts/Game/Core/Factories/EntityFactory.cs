using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Content;
using Game.Core.Extensions;
using Game.Core.Lifecycle.Extensions;
using Game.Core.Logging;
using Game.Core.Reflection;
using Game.Features.Entities.Content;
using Game.Features.Sessions.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Utils;
using VContainer;

namespace Game.Core.Factories
{
    [AutoRegister]
    public class EntityFactory : IEntityFactory
    {
        [Inject]
        private IContentManager m_contentManager;

        [Inject]
        private ISessionAddressableManager m_addressableManager;

        [Inject]
        private IReflectionManager m_reflectionManager;

        [Inject]
        private IExtensionExecutor m_extensionExecutor;

        public async UniTask<Entity> CreateEntityAsync(ReferenceWrapper<EntityManager> entityManager, string contentId)
        {
            var contentEntity = m_contentManager.Get<ContentEntity>(contentId);
            if (contentEntity == null)
            {
                GameLogger.Error($"Entity content not found: {contentId}");
                return default;
            }

            var entity = entityManager.Value.CreateEntity();
            entity.AddReference(contentEntity);

            await m_extensionExecutor.ExecuteAsync<IEntityCreatedExtension>(entity, contentEntity,
                extension => extension.OnEntityCreated(entity, contentEntity));

            GameLogger.Log($"Created entity: {contentId}");

            return entity;
        }

        public async UniTask DestroyEntityAsync(ReferenceWrapper<EntityManager> entityManager, Entity entity)
        {
            if (!entity.IsAlive())
            {
                GameLogger.Warning("Attempted to destroy invalid entity");
                return;
            }

            var contentEntity = entity.GetReference<ContentEntity>();
            if (contentEntity == null)
            {
                GameLogger.Warning("Entity has no ContentEntity reference");
            }

            await m_extensionExecutor.ExecuteAsync<IEntityDestroyedExtension>(entity, contentEntity,
                extension => extension.OnEntityDestroyed(entity, contentEntity));

            entityManager.Value.DestroyEntity(entity);

            GameLogger.Log($"Destroyed entity: {contentEntity?.id ?? "unknown"}");
        }
    }
}