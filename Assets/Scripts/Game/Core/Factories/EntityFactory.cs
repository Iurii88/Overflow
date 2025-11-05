using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Content;
using Game.Core.Lifecycle;
using Game.Core.Logging;
using Game.Core.Reflection;
using Game.Core.Reflection.Attributes;
using Game.Features.Entities.Content;
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
        private IAddressableManager m_addressableManager;

        [Inject]
        private IReflectionManager m_reflectionManager;

        [Inject]
        private IObjectResolver m_resolver;

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
            var extensions = m_resolver.Resolve<IReadOnlyList<IEntityCreatedExtension>>();
            for (var i = 0; i < extensions.Count; i++)
                await extensions[i].OnEntityCreated(entity, contentEntity);

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

            var extensions = m_resolver.Resolve<IReadOnlyList<IEntityDestroyedExtension>>();
            for (var i = 0; i < extensions.Count; i++)
                await extensions[i].OnEntityDestroyed(entity, contentEntity);

            entityManager.Value.DestroyEntity(entity);

            GameLogger.Log($"Destroyed entity: {contentEntity?.id ?? "unknown"}");
        }
    }
}