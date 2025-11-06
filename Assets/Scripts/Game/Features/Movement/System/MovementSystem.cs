using Game.Core.Factories;
using Game.Features.Movement.Components;
using UnityEngine;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Movement.System
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class MovementSystem : SystemBase
    {
        private EntityQuery m_movementQuery;

        [Inject]
        private IEntityFactory m_entityFactory;

        public override void OnAwake()
        {
            m_movementQuery = CreateQuery().With<Velocity>();
        }

        public override void OnUpdate()
        {
            m_movementQuery.ForEach((ref Entity entity, ref Velocity velocity) =>
            {
                if (!entity.TryGetReference(out GameObject gameObject))
                    return;

                var transform = gameObject.transform;
                var movement = new Vector3(velocity.value.x, velocity.value.y, 0) * world.deltaTime;
                transform.position += movement;
            });
        }
    }
}