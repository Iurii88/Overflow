using System;
using Game.Core.EntityControllers;
using Game.Core.UI;
using Game.Core.UI.Blackboards;
using Game.Features.Stats.Consts;
using Game.Features.Stats.Controllers;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Features.Stats.UI
{
    public class HealthBarViewComponent : AEntityViewComponent
    {
        [SerializeField]
        private Image fillImage;

        [SerializeField]
        private float animationDuration = 0.3f;

        public BlackboardViewParameter<int> health;

        private IDisposable m_animationDisposable;

        protected override void Subscribe()
        {
            health.OnVariableChanged += HealthOnOnVariableChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_animationDisposable?.Dispose();
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

        public override void OnInitialize()
        {
            base.OnInitialize();
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if (!Application.isPlaying || entity.IsAlive())
                return;

            var statsController = entity.GetOrCreateController<StatsController>();

            if (!statsController.TryGetMaxStat(StatsConstants.Health, out var maxHealth))
                return;

            var targetFillAmount = maxHealth <= 0f ? 0f : health.Value / maxHealth;

            m_animationDisposable?.Dispose();

            if (Mathf.Abs(fillImage.fillAmount - targetFillAmount) < 0.001f)
                return;

            var startFillAmount = fillImage.fillAmount;

            m_animationDisposable = Observable.EveryUpdate()
                .Select(_ => Time.deltaTime)
                .Scan(0f, (elapsed, deltaTime) => elapsed + deltaTime)
                .TakeWhile(elapsed => elapsed < animationDuration)
                .Subscribe(elapsed =>
                {
                    var t = Mathf.Clamp01(elapsed / animationDuration);
                    fillImage.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, t);
                });
        }
    }
}