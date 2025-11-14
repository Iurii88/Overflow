using Game.Core.EntityControllers;
using Game.Core.Logging;
using Game.Core.UI.Blackboards;
using Game.Features.Stats.Consts;
using UnsafeEcs.Core.Entities;

namespace Game.Features.Stats.Controllers
{
    public class DamageController : EntityController
    {
        private StatsController m_statsController;
        private Blackboard m_blackboard;

        public override void OnAwake(Entity entity)
        {
            base.OnAwake(entity);
            m_statsController = entity.GetController<StatsController>();

            if (entity.TryGetReference(out Blackboard blackboard))
                m_blackboard = blackboard;
        }

        private void UpdateHealthInBlackboard()
        {
            if (m_blackboard == null)
                return;

            var health = m_statsController.GetStat(StatsConstants.Health);
            m_blackboard.Set(StatsConstants.Health, (int)health);
        }

        public float GetHealth()
        {
            return m_statsController.GetStat(StatsConstants.Health);
        }

        public bool TryGetHealth(out float health)
        {
            health = 0f;
            return m_statsController.TryGetStat(StatsConstants.Health, out health);
        }

        public float GetMaxHealth()
        {
            return m_statsController.GetMaxStat(StatsConstants.Health);
        }

        public bool TryGetMaxHealth(out float maxHealth)
        {
            maxHealth = 0f;
            return m_statsController.TryGetMaxStat(StatsConstants.Health, out maxHealth);
        }

        public bool IsDead()
        {
            return GetHealth() <= 0f;
        }

        public void ResetHealthToMax()
        {
            var currentHealth = m_statsController.GetStat(StatsConstants.Health);
            m_statsController.ResetToMax(StatsConstants.Health);
            var newHealth = m_statsController.GetStat(StatsConstants.Health);

            UpdateHealthInBlackboard();
            GameLogger.Log($"Entity {Entity.id} health reset to max. Health: {currentHealth} -> {newHealth}");
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f)
            {
                GameLogger.Warning($"TakeDamage called with non-positive amount: {amount} on entity {Entity.id}");
                return;
            }

            var currentHealth = m_statsController.GetStat(StatsConstants.Health);
            m_statsController.ModifyStat(StatsConstants.Health, -amount);
            var newHealth = m_statsController.GetStat(StatsConstants.Health);

            UpdateHealthInBlackboard();
            GameLogger.Log($"Entity {Entity.id} took {amount} damage. Health: {currentHealth} -> {newHealth}");

            if (newHealth <= 0f && currentHealth > 0f)
                OnDeath();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f)
            {
                GameLogger.Warning($"Heal called with non-positive amount: {amount} on entity {Entity.id}");
                return;
            }

            var currentHealth = m_statsController.GetStat(StatsConstants.Health);
            m_statsController.ModifyStat(StatsConstants.Health, amount);
            var newHealth = m_statsController.GetStat(StatsConstants.Health);

            UpdateHealthInBlackboard();
            GameLogger.Log($"Entity {Entity.id} healed {amount}. Health: {currentHealth} -> {newHealth}");
        }

        public void SetHealth(float value)
        {
            var currentHealth = m_statsController.GetStat(StatsConstants.Health);
            m_statsController.SetStat(StatsConstants.Health, value);
            var newHealth = m_statsController.GetStat(StatsConstants.Health);

            UpdateHealthInBlackboard();

            if (newHealth <= 0f && currentHealth > 0f)
                OnDeath();
        }

        protected virtual void OnDeath()
        {
            GameLogger.Log($"Entity {Entity.id} has died");
        }
    }
}