using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Content.Properties.Filters;
using Game.Core.EntityControllers;
using Game.Core.Extensions;
using Game.Core.Extensions.Filters;
using Game.Core.Lifecycle.Extensions;
using Game.Core.Reflection.Attributes;
using Game.Core.UI.Blackboards;
using Game.Features.Entities.Content;
using Game.Features.Stats.Consts;
using Game.Features.Stats.Content;
using Game.Features.Stats.Controllers;
using UnsafeEcs.Core.Entities;

namespace Game.Features.Stats.Extensions
{
    [AutoRegister]
    [ExtensionPriority(100)]
    public class StatsBlackboardExtension : IEntityCreatedExtension
    {
        public IReadOnlyList<IExtensionFilter> Filters { get; } = new List<IExtensionFilter>
        {
            new HasPropertyFilter<StatsContentProperty>()
        };

        public UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            if (!entity.TryGetReference(out Blackboard blackboard))
                return UniTask.CompletedTask;

            var statsController = entity.GetController<StatsController>();
            if (statsController == null)
                return UniTask.CompletedTask;

            if (statsController.TryGetStat(StatsConstants.Health, out var health))
                blackboard.Set(StatsConstants.Health, (int)health);

            if (statsController.TryGetMaxStat(StatsConstants.Health, out var maxHealth))
                blackboard.Set(StatsConstants.MaxHealth, (int)maxHealth);

            return UniTask.CompletedTask;
        }
    }
}