using System;
using Game.Core.UI;
using Game.Core.UI.Blackboards;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Features.Stats.UI
{
    public class ProgressViewComponent : AEntityViewComponent
    {
        [SerializeField]
        private Image fillImage;

        [SerializeField]
        private float animationDuration = 0.3f;

        public BlackboardViewParameter<int> current;
        public BlackboardViewParameter<int> max;

        private IDisposable m_animationDisposable;

        protected override void Subscribe()
        {
            current.OnVariableChanged += OnVariableChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_animationDisposable?.Dispose();
        }

        private void OnVariableChanged(BlackboardVariable<int> healthVariable)
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
            if (!Application.isPlaying)
                return;

            var targetProgress = max.Value <= 0f ? 0f : (float)current.Value / max.Value;

            m_animationDisposable?.Dispose();

            if (Mathf.Abs(fillImage.fillAmount - targetProgress) < 0.001f)
                return;

            var startProgress = fillImage.fillAmount;

            m_animationDisposable = Observable.EveryUpdate()
                .Select(_ => Time.deltaTime)
                .Scan(0f, (elapsed, deltaTime) => elapsed + deltaTime)
                .TakeWhile(elapsed => elapsed < animationDuration)
                .Subscribe(elapsed =>
                {
                    var t = Mathf.Clamp01(elapsed / animationDuration);
                    fillImage.fillAmount = Mathf.Lerp(startProgress, targetProgress, t);
                });
        }
    }
}