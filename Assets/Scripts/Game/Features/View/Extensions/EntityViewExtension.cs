using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Lifecycle;
using Game.Core.Logging;
using Game.Core.Reflection.Attributes;
using Game.Features.Entities.Content;
using Game.Features.View.Content;
using UnityEngine;
using UnsafeEcs.Core.Entities;
using VContainer;

namespace Game.Features.View.Extensions
{
    [AutoRegister]
    public class EntityViewExtension : IEntityCreatedExtension, IEntityDestroyedExtension
    {
        [Inject]
        private IAddressableManager m_addressableManager;

        public async UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            var viewProperty = contentEntity.GetProperty<ViewContentProperty>();
            if (viewProperty == null)
                GameLogger.Error($"Entity {contentEntity.id} has no VIEW property");

            var prefab = await m_addressableManager.LoadAssetAsync<GameObject>(viewProperty.assetPath);
            if (prefab == null)
                GameLogger.Error($"Failed to load prefab for entity {contentEntity.id} at path: {viewProperty.assetPath}");

            var gameObject = Object.Instantiate(prefab);
            gameObject.name = contentEntity.id;
        }

        public UniTask OnEntityDestroyed(Entity entity, ContentEntity contentEntity)
        {
            return UniTask.CompletedTask;
        }
    }
}