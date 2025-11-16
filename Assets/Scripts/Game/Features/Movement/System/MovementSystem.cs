using Game.Core.Factories;
using Game.Features.Movement.Components;
using Game.Features.Pause;
using Game.Features.Pause.Groups;
using UnityEngine;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Components.Managed;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Movement.System
{
    [UpdateInGroup(typeof(TimeSystemGroup))]
    public class MovementSystem : SystemBase
    {
        private EntityQuery m_movementQuery;

        [Inject]
        private IEntityFactory m_entityFactory;

        [Inject]
        private ISessionTime sessionTime;

        public override void OnAwake()
        {
            m_movementQuery = CreateQuery().With<Velocity, ManagedRef<Transform>>();
        }

        public override void OnUpdate()
        {
            m_movementQuery.ForEach(sessionTime.DeltaTime, (float dt, ref Entity _, ref Velocity velocity, ref ManagedRef<Transform> transformRef) =>
            {
                var movement = new Vector3(velocity.value.x, velocity.value.y, 0) * dt;
                transformRef.Get().position += movement;
            });
        }
    }
}