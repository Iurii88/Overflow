using Game.Core.EntityControllers;
using Game.Core.UI;
using Game.Core.UI.Blackboard;
using Game.Features.Stats.Consts;
using Game.Features.Stats.Controllers;
using UnityEngine;
using UnityEngine.UI;
using UnsafeEcs.Core.Entities;

namespace Game.Features.Stats.UI
{
    public class HealthBarViewComponent : AViewComponent
    {
        [SerializeField]
        private Image fillImage;

        [SerializeField]
        private float updateSpeed = 5f;

        public BlackboardViewParameter<Entity> entity;
        public BlackboardViewParameter<int> health;

        private float m_currentFillAmount;
        private float m_targetFillAmount;

        protected override void Subscribe()
        {
            health.OnVariableChanged += HealthOnOnVariableChanged;
        }

        private void HealthOnOnVariableChanged(BlackboardVariable<int> healthVariable)
        {
            UpdateHealthBar();
        }

        protected override void Reset()
        {
            base.Reset();
            fillImage = GetComponentInChildren<Image>();
        }

        private void UpdateHealthBar()
        {
            if (Application.isEditor)
                return;

            var statsController = entity.Value.GetOrCreateController<StatsController>();
            if (!statsController.TryGetStat(StatsConstants.Health, out var currentHealth))
                return;

            if (!statsController.TryGetMaxStat(StatsConstants.Health, out var maxHealth))
                return;

            if (maxHealth <= 0f)
            {
                m_targetFillAmount = 0f;
                return;
            }

            m_targetFillAmount = currentHealth / maxHealth;
            if (!(Mathf.Abs(m_currentFillAmount - m_targetFillAmount) > 0.001f))
                return;

            m_currentFillAmount = Mathf.Lerp(m_currentFillAmount, m_targetFillAmount, Time.deltaTime * updateSpeed);
            fillImage.fillAmount = m_currentFillAmount;
        }
    }
}