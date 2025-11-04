using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Content;
using Game.Core.Logging;
using Game.Core.Reflection.Attributes;
using Game.Features.Entities.Content;
using Game.Features.View.Content;
using UnityEngine;
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

        public async UniTask<Entity> CreateEntityAsync(ReferenceWrapper<EntityManager> entityManager, string contentId)
        {
            var contentEntity = m_contentManager.Get<ContentEntity>(contentId);
            if (contentEntity == null)
            {
                GameLogger.Error($"Entity content not found: {contentId}");
                return default;
            }

            var viewProperty = contentEntity.GetProperty<ViewContentProperty>();
            if (viewProperty == null)
            {
                GameLogger.Error($"Entity {contentId} has no VIEW property");
                return default;
            }

            var prefab = await m_addressableManager.LoadAssetAsync<GameObject>(viewProperty.assetPath);
            if (prefab == null)
            {
                GameLogger.Error($"Failed to load prefab for entity {contentId} at path: {viewProperty.assetPath}");
                return default;
            }

            var gameObject = Object.Instantiate(prefab);
            gameObject.name = contentId;

            var entity = entityManager.Value.CreateEntity();
            entity.AddReference(gameObject);

            GameLogger.Log($"Created entity: {contentId}");

            return entity;
        }
    }
}