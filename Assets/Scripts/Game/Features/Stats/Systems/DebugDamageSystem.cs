using Game.Core.EntityControllers;
using Game.Core.Logging;
using Game.Features.Players.Common.Components;
using Game.Features.Stats.Controllers;
using UnityEngine;
using UnsafeEcs.Additions.Groups;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;

namespace Game.Features.Stats.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class DebugDamageSystem : SystemBase
    {
        public override SystemUpdateMask UpdateMask => SystemUpdateMask.Update;

        private EntityQuery m_playerQuery;
        private float m_timeSinceLastDamage;
        private const float DamageInterval = 1f;
        private const float DamageAmount = 10f;

        public override void OnAwake()
        {
            m_playerQuery = CreateQuery().With<PlayerTag>();
        }

        public override void OnUpdate()
        {
            m_timeSinceLastDamage += Time.deltaTime;

            if (m_timeSinceLastDamage >= DamageInterval)
            {
                m_timeSinceLastDamage = 0f;
                DamagePlayer();
            }
        }

        private void DamagePlayer()
        {
            m_playerQuery.ForEach((ref Entity entity) =>
            {
                var damageController = entity.GetOrCreateController<DamageController>();
                damageController.TakeDamage(DamageAmount);
                GameLogger.Log($"[DebugDamageSystem] Applied {DamageAmount} damage to player");
            });
        }
    }
}