using Game.Features.Movement.Components;
using Game.Features.Movement.Jobs;
using Game.Features.Pause;
using Game.Features.Pause.Groups;
using Unity.Jobs;
using UnsafeEcs.Core.Bootstrap.Attributes;
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
        private ISessionTime m_sessionTime;

        public override void OnAwake()
        {
            m_movementQuery = CreateQuery().With<Velocity, Position>();
        }

        public override void OnUpdate()
        {
            var entities = m_movementQuery.Fetch();
            var positions = GetComponentArray<Position>();
            var velocities = GetComponentArray<Velocity>();
            new MovementJob
            {
                entities = entities,
                positions = positions,
                velocities = velocities,
                deltaTime = m_sessionTime.DeltaTime
            }.Schedule(entities.Length, 64).Complete();
        }
    }
}