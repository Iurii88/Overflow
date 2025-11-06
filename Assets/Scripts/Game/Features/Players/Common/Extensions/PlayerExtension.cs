using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Content.Properties.Filters;
using Game.Core.Extensions.Filters;
using Game.Core.Lifecycle;
using Game.Core.Pooling;
using Game.Core.Reflection.Attributes;
using Game.Features.Entities.Content;
using Game.Features.Players.Common.Components;
using Game.Features.View.Content;
using UnsafeEcs.Core.Entities;
using VContainer;

namespace Game.Features.View.Extensions
{
    [AutoRegister]
    public class PlayerExtension : IEntityCreatedExtension
    {
        [Inject]
        private IAsyncPoolManager m_poolManager;

        public IReadOnlyList<IExtensionFilter> Filters { get; } = new List<IExtensionFilter>
        {
            new HasPropertyFilter<PlayerContentProperty>()
        };

        public UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            entity.AddComponent<PlayerTag>();
            return UniTask.CompletedTask;
        }
    }
}