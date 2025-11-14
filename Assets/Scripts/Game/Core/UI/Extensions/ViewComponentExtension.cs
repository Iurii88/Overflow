using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Content.Properties.Filters;
using Game.Core.Extensions.Filters;
using Game.Core.Lifecycle.Extensions;
using Game.Core.Logging;
using Game.Core.Pooling;
using Game.Core.Reflection.Attributes;
using Game.Core.UI.Content;
using Game.Core.UI.Data;
using Game.Core.UI.Layers;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;
using VContainer;
using ZLinq;

namespace Game.Core.UI.Extensions
{
    [AutoRegister]
    public class ViewComponentExtension : IEntityCreatedExtension, IEntityDestroyedExtension
    {
        [Inject]
        private IAsyncPoolManager m_poolManager;

        [Inject]
        private UILayerManager m_layerManager;

        public IReadOnlyList<IExtensionFilter> Filters { get; } = new List<IExtensionFilter>
        {
            new HasPropertyFilter<ViewComponentContentProperty>()
        };

        public async UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            var viewComponentProperties = contentEntity.GetProperties<ViewComponentContentProperty>();
            var viewComponents = new ViewComponentsList();

            foreach (var property in viewComponentProperties.AsValueEnumerable())
            {
                if (!property.enabled)
                    continue;

                if (string.IsNullOrEmpty(property.assetPath))
                {
                    GameLogger.Warning($"ViewComponentContentProperty has empty assetPath on entity {contentEntity.id}");
                    continue;
                }

                var layerTransform = m_layerManager.Get(property.layer);
                if (layerTransform == null)
                {
                    GameLogger.Error($"UILayer {property.layer} not registered. Cannot instantiate view component for entity {contentEntity.id}");
                    continue;
                }

                var viewObject = await m_poolManager.GetGameObjectAsync(property.assetPath);
                if (viewObject == null)
                {
                    GameLogger.Error($"Failed to load view component prefab for entity {contentEntity.id} at path: {property.assetPath}");
                    continue;
                }

                viewObject.transform.SetParent(layerTransform, false);
                viewObject.SetActive(property.activeOnStart);

                var viewComponent = viewObject.GetComponent<AViewComponent>();
                if (viewComponent is AEntityViewComponent entityViewComponent)
                {
                    entityViewComponent.entity = entity;
                }
                else
                {
                    var blackboard = viewObject.GetComponent<Blackboard.Blackboard>();
                    if (blackboard != null)
                    {
                        blackboard.Set("ENTITY", entity);
                    }
                }

                viewComponents.Add(viewObject);
                viewComponent.OnInitialize();
                GameLogger.Log($"Instantiated view component for entity {contentEntity.id} on layer {property.layer}");
            }

            if (viewComponents.Count > 0)
                entity.AddReference(viewComponents);
        }

        public UniTask OnEntityDestroyed(Entity entity, ContentEntity contentEntity)
        {
            if (!entity.TryGetReference(out ViewComponentsList viewComponents))
                return UniTask.CompletedTask;

            foreach (var viewComponent in viewComponents.AsValueEnumerable())
            {
                if (viewComponent != null)
                    m_poolManager.Release(viewComponent);
            }

            entity.RemoveReference<ViewComponentsList>();
            return UniTask.CompletedTask;
        }
    }
}