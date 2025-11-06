using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Content.Properties.Filters;
using Game.Core.Extensions.Filters;
using Game.Core.Lifecycle;
using Game.Core.Pooling;
using Game.Core.Reflection.Attributes;
using Game.Features.Entities.Content;
using Game.Features.Movement.Components;
using Game.Features.Movement.Content;
using UnsafeEcs.Core.Entities;
using VContainer;

namespace Game.Features.Movement.Extensions
{
    [AutoRegister]
    public class MovableExtension : IEntityCreatedExtension
    {
        [Inject]
        private IAsyncPoolManager m_poolManager;

        public IReadOnlyList<IExtensionFilter> Filters { get; } = new List<IExtensionFilter>
        {
            new HasPropertyFilter<MovableContentProperty>()
        };

        public UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            var movableContentProperty = contentEntity.GetProperty<MovableContentProperty>();
            entity.SetComponent(new Speed { value = movableContentProperty.baseSpeed });
            entity.SetComponent<Velocity>();
            return UniTask.CompletedTask;
        }
    }
}