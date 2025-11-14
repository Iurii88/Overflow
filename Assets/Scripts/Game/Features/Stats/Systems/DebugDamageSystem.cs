using System;
using Game.Core.EntityControllers;
using Game.Core.Logging;
using Game.Features.Pause;
using Game.Features.Pause.Groups;
using Game.Features.Players.Common.Components;
using Game.Features.Stats.Controllers;
using R3;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Stats.Systems
{
    [UpdateInGroup(typeof(PauseAwareSystemGroup))]
    public class DebugDamageSystem : SystemBase
    {
        private const float DamageInterval = 1f;
        private const float DamageAmount = 10f;

        private EntityQuery m_playerQuery;
        private IDisposable m_damageSubscription;

        [Inject]
        private IGameDeltaTime m_gameDeltaTime;

        public override void OnAwake()
        {
            m_playerQuery = CreateQuery().With<PlayerTag>();

            m_damageSubscription = Observable.EveryUpdate()
                .Select(_ => m_gameDeltaTime.DeltaTime)
                .Scan((elapsed: 0f, shouldFire: false), (state, deltaTime) =>
                {
                    var newElapsed = state.elapsed + deltaTime;
                    return newElapsed >= DamageInterval ? (elapsed: newElapsed - DamageInterval, shouldFire: true) : (elapsed: newElapsed, shouldFire: false);
                })
                .Where(state => state.shouldFire)
                .Subscribe(_ => { DamagePlayer(); });
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            m_damageSubscription?.Dispose();
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