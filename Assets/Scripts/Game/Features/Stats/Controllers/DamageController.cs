using Game.Core.EntityControllers;
using Game.Core.Logging;
using UnsafeEcs.Core.Entities;

namespace Game.Features.Stats.Controllers
{
    public class DamageController : EntityController
    {
        private StatsController m_statsController;

        public override void OnAwake(Entity entity)
        {
            base.OnAwake(entity);
            m_statsController = entity.GetController<StatsController>();
        }

        public float GetHealth()
        {
            if (m_statsController == null)
                return 0f;

            return m_statsController.GetStat(StatsConstants.Health);
        }

        public bool TryGetHealth(out float health)
        {
            health = 0f;
            if (m_statsController == null)
                return false;

            return m_statsController.TryGetStat(StatsConstants.Health, out health);
        }

        public bool IsDead()
        {
            return GetHealth() <= 0f;
        }

        public void TakeDamage(float amount)
        {
            if (m_statsController == null)
                return;

            if (amount <= 0f)
            {
                GameLogger.Warning($"TakeDamage called with non-positive amount: {amount} on entity {Entity.id}");
                return;
            }

            var currentHealth = m_statsController.GetStat(StatsConstants.Health);
            m_statsController.ModifyStat(StatsConstants.Health, -amount);
            var newHealth = m_statsController.GetStat(StatsConstants.Health);

            GameLogger.Log($"Entity {Entity.id} took {amount} damage. Health: {currentHealth} -> {newHealth}");

            if (newHealth <= 0f && currentHealth > 0f)
            {
                OnDeath();
            }
        }

        public void Heal(float amount)
        {
            if (m_statsController == null)
                return;

            if (amount <= 0f)
            {
                GameLogger.Warning($"Heal called with non-positive amount: {amount} on entity {Entity.id}");
                return;
            }

            var currentHealth = m_statsController.GetStat(StatsConstants.Health);
            m_statsController.ModifyStat(StatsConstants.Health, amount);
            var newHealth = m_statsController.GetStat(StatsConstants.Health);

            GameLogger.Log($"Entity {Entity.id} healed {amount}. Health: {currentHealth} -> {newHealth}");
        }

        public void SetHealth(float value)
        {
            if (m_statsController == null)
                return;

            var currentHealth = m_statsController.GetStat(StatsConstants.Health);
            m_statsController.SetStat(StatsConstants.Health, value);
            var newHealth = m_statsController.GetStat(StatsConstants.Health);

            if (newHealth <= 0f && currentHealth > 0f)
            {
                OnDeath();
            }
        }

        protected virtual void OnDeath()
        {
            GameLogger.Log($"Entity {Entity.id} has died");
        }
    }
}
