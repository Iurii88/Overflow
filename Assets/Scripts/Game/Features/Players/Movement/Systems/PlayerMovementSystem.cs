using Game.Core.Factories;
using Game.Features.Movement.Components;
using Game.Features.Players.Common.Components;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private InputAction m_moveAction;

        [Inject]
        private IEntityFactory m_entityFactory;

        [Inject]
        private InputActionAsset m_inputActionAsset;

        public override void OnAwake()
        {
            m_playerQuery = CreateQuery()
                .With<PlayerTag>()
                .With<Velocity>()
                .With<Speed>();

            m_moveAction = m_inputActionAsset.FindAction("Player/Move");
            m_moveAction.Enable();
        }

        public override void OnDestroy()
        {
            m_moveAction?.Disable();
        }

        public override void OnUpdate()
        {
            HandlePlayerInput();
        }

        private void HandlePlayerInput()
        {
            var moveInput = m_moveAction.ReadValue<Vector2>();
            var input = new float2(moveInput.x, moveInput.y);

            if (math.lengthsq(input) > 0)
                input = math.normalize(input);

            m_playerQuery.ForEach((ref Entity _, ref Velocity velocity, ref Speed speed) =>
            {
                velocity.value = input * speed.value;
            });
        }
    }
}