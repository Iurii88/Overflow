using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Content.Properties.Filters;
using Game.Core.Extensions.Filters;
using Game.Core.Lifecycle.Extensions;
using Game.Core.Logging;
using Game.Core.Pooling;
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
        private IAsyncPoolManager m_poolManager;

        public IReadOnlyList<IExtensionFilter> Filters { get; } = new List<IExtensionFilter>
        {
            new HasPropertyFilter<ViewContentProperty>()
        };

        public async UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            var viewProperty = contentEntity.GetProperty<ViewContentProperty>();
            var gameObject = await m_poolManager.GetGameObjectAsync(viewProperty.assetPath);

            if (gameObject == null)
            {
                GameLogger.Error($"Failed to load prefab for entity {contentEntity.id} at path: {viewProperty.assetPath}");
                return;
            }

            gameObject.name = contentEntity.id;
            entity.AddReference(gameObject);
            entity.AddReference(gameObject.transform);
        }

        public UniTask OnEntityDestroyed(Entity entity, ContentEntity contentEntity)
        {
            m_poolManager.Release(entity.GetReference<GameObject>());
            entity.RemoveReference<GameObject>();
            entity.RemoveReference<Transform>();
            return UniTask.CompletedTask;
        }
    }
}