using Game.Features.Enemies.Common.Components;
using Game.Features.Movement.Components;
using Game.Features.Movement.System;
using Game.Features.Pause.Groups;
using Game.Features.Players.Common.Components;
using Unity.Mathematics;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;

namespace Game.Features.Enemies.AI.Systems
{
    [UpdateInGroup(typeof(TimeSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    public class EnemyFollowSystem : SystemBase
    {
        private EntityQuery m_enemyQuery;
        private EntityQuery m_playerQuery;

        public override void OnAwake()
        {
            m_enemyQuery = CreateQuery()
                .With<EnemyTag>()
                .With<Position>()
                .With<Velocity>()
                .With<Speed>();

            m_playerQuery = CreateQuery()
                .With<PlayerTag>()
                .With<Position>();
        }

        public override void OnUpdate()
        {
            var playerPosition = default(float2);
            m_playerQuery.ForEach((ref Entity _, ref Position position) =>
            {
                playerPosition = position.value;
            });

            m_enemyQuery.ForEach(playerPosition, (float2 targetPos, ref Entity _, ref Position position, ref Velocity velocity, ref Speed speed) =>
            {
                var direction = targetPos - position.value;
                var distanceSq = math.lengthsq(direction);

                if (distanceSq > 0.01f)
                {
                    direction = math.normalize(direction);
                    velocity.value = direction * speed.value;
                }
                else
                {
                    velocity.value = float2.zero;
                }
            });
        }
    }
}