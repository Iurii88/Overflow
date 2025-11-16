using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Core.Content.Properties.Filters;
using Game.Core.Extensions.Filters;
using Game.Core.Lifecycle.Extensions;
using Game.Features.Camera.Components;
using Game.Features.Camera.Content;
using Game.Features.Entities.Content;
using Game.Features.Sessions.Attributes;
using UnsafeEcs.Core.Entities;

namespace Game.Features.Camera.Extensions
{
    [AutoRegister]
    public class CameraFollowingExtension : IEntityCreatedExtension
    {
        public IReadOnlyList<IExtensionFilter> Filters { get; } = new List<IExtensionFilter>
        {
            new HasPropertyFilter<CameraFollowingContentProperty>()
        };

        public UniTask OnEntityCreated(Entity entity, ContentEntity contentEntity)
        {
            var cameraFollowingProperty = contentEntity.GetProperty<CameraFollowingContentProperty>();
            entity.SetComponent(new CameraTarget
            {
                offset = cameraFollowingProperty.offset,
                smoothSpeed = cameraFollowingProperty.smoothSpeed
            });
            return UniTask.CompletedTask;
        }
    }
}