using Cysharp.Threading.Tasks;
using Game.Core.Content.Properties.Filters;
using Game.Core.Extensions.Filters;
using Game.Core.Lifecycle.Extensions;
using Game.Features.Enemies.Common.Components;
using Game.Features.Enemies.Common.Content;
using Game.Features.Entities.Content;
using Game.Features.Sessions.Attributes;
using System.Collections.Generic;
using UnsafeEcs.Core.Entities;

namespace Game.Features.Enemies.Common.Extensions
{
    [AutoRegister]
    public class EnemyExtension : IEntityCreatedExtension
    {
        public IReadOnlyList<IExtensionFilter> Filters { get; } = new List<IExtensionFilter>
        {
            new HasPropertyFilter<EnemyContentProperty>()
        };

        public UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            entity.AddComponent<EnemyTag>();
            return UniTask.CompletedTask;
        }
    }
}