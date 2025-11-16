using Game.Features.Movement.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnsafeEcs.Core.Components;
using UnsafeEcs.Core.Entities;

namespace Game.Features.Movement.Jobs
{
    [BurstCompile]
    public struct MovementJob : IJobParallelFor
    {
        [ReadOnly]
        public UnsafeList<Entity> entities;

        [ReadOnly]
        public ComponentArray<Velocity> velocities;

        public ComponentArray<Position> positions;
        public float deltaTime;

        public void Execute(int index)
        {
            var entity = entities[index];
            ref var velocity = ref velocities.Get(entity);
            ref var position = ref positions.Get(entity);
            position.value += velocity.value * deltaTime;
        }
    }
}