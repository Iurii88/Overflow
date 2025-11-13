using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Content.Properties.Filters;
using Game.Core.EntityControllers;
using Game.Core.Extensions.Filters;
using Game.Core.Lifecycle.Extensions;
using Game.Core.Reflection.Attributes;
using Game.Features.Entities.Content;
using Game.Features.Stats.Content;
using Game.Features.Stats.Controllers;
using UnsafeEcs.Core.Entities;

namespace Game.Features.Stats.Extensions
{
    [AutoRegister]
    public class StatsExtension : IEntityCreatedExtension
    {
        public IReadOnlyList<IExtensionFilter> Filters { get; } = new List<IExtensionFilter>
        {
            new HasPropertyFilter<StatsContentProperty>()
        };

        public UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            var statsProperty = contentEntity.GetProperty<StatsContentProperty>();
            var statsController = entity.AddController<StatsController>();
            statsController.InitializeStats(statsProperty.stats);

            return UniTask.CompletedTask;
        }
    }
}
