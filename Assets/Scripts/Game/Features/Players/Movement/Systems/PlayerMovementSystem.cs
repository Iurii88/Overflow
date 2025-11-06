using Game.Core.Factories;
using Game.Features.Movement.Components;
using Game.Features.Players.Common.Components;
using Unity.Mathematics;
using UnityEngine;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Movement.System
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(MovementSystem))]
    public class PlayerMovementSystem : SystemBase
    {
        private EntityQuery m_playerQuery;
        private EntityQuery m_movementQuery;

        [Inject]
        private IEntityFactory m_entityFactory;

        public override void OnAwake()
        {
            m_playerQuery = CreateQuery()
                .With<PlayerTag>()
                .With<Velocity>()
                .With<Speed>();
        }

        public override void OnUpdate()
        {
            HandlePlayerInput();
        }

        private void HandlePlayerInput()
        {
            m_playerQuery.ForEach((ref Entity entity, ref Velocity velocity, ref Speed speed) =>
            {
                var input = new float2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                );

                if (math.lengthsq(input) > 0)
                    input = math.normalize(input);

                velocity.value = input * speed.value;
            });
        }
    }
}