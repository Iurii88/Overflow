using Game.Features.Movement.Components;
using Game.Features.Pause.Groups;
using UnityEngine;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Components.Managed;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;

namespace Game.Features.Movement.System
{
    [UpdateInGroup(typeof(TimeSystemGroup))]
    [UpdateAfter(typeof(MovementSystem))]
    public class TransformSyncSystem : SystemBase
    {
        private EntityQuery m_syncQuery;

        public override void OnAwake()
        {
            m_syncQuery = CreateQuery().With<Position, ManagedRef<Transform>>();
        }

        public override void OnUpdate()
        {
            m_syncQuery.ForEach((ref Entity _, ref Position position, ref ManagedRef<Transform> transformRef) =>
            {
                var transform = transformRef.Get();
                transform.position = new Vector3(position.value.x, position.value.y, transform.position.z);
            });
        }
    }
}