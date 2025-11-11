using Game.Core.UI.Blackboard;
using UnityEngine;
using UnsafeEcs.Core.Entities;

namespace Game.Core.UI.ViewModules
{
    [RequireComponent(typeof(RectTransform))]
    public class WorldPositionSyncViewComponent : AViewComponent
    {
        public BlackboardViewParameter<Entity> entity;

        [SerializeField]
        private Vector2 worldOffset = Vector2.zero;

        private RectTransform m_rectTransform;
        private Camera m_camera;

        protected override void Subscribe()
        {
            entity.OnVariableChanged += EntityOnOnVariableChanged;
        }

        private void EntityOnOnVariableChanged(BlackboardVariable<Entity> entityVariable)
        {
        }

        protected override void Awake()
        {
            base.Awake();
            m_rectTransform = GetComponent<RectTransform>();
            m_camera = Camera.main;
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (!Application.isPlaying)
                return;

            if (!entity.Value.IsAlive())
                return;

            var entityTransform = entity.Value.GetReference<Transform>();
            if (entityTransform == null)
                return;

            if (m_camera == null)
                return;

            var worldPosition = (Vector2)entityTransform.position + worldOffset;
            var screenPosition = m_camera.WorldToScreenPoint(worldPosition);

            var parentRect = m_rectTransform.parent as RectTransform;
            if (parentRect == null)
            {
                m_rectTransform.position = worldPosition;
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                screenPosition,
                null,
                out var localPoint
            );

            m_rectTransform.anchoredPosition = localPoint;
        }
    }
}