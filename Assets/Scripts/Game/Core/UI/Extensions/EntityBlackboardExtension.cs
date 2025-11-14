using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Extensions;
using Game.Core.Extensions.Filters;
using Game.Core.Lifecycle.Extensions;
using Game.Core.Reflection.Attributes;
using Game.Core.UI.Blackboards;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.UI.Extensions
{
    [AutoRegister]
    [ExtensionPriority(-100)]
    public class EntityBlackboardExtension : IEntityCreatedExtension, IEntityDestroyedExtension
    {
        public IReadOnlyList<IExtensionFilter> Filters { get; } = Array.Empty<IExtensionFilter>();

        public UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            var blackboard = new Blackboard();
            blackboard.Set("ENTITY", entity);
            entity.AddReference(blackboard);

            return UniTask.CompletedTask;
        }

        public UniTask OnEntityDestroyed(Entity entity, ContentEntity contentEntity)
        {
            entity.RemoveReference<Blackboard>();
            return UniTask.CompletedTask;
        }
    }
}