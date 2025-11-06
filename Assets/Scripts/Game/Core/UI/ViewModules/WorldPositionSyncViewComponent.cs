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
        private Vector3 worldOffset = new(0f, 1f, 0f);

        protected override void Subscribe()
        {
            entity.OnVariableChanged += EntityOnOnVariableChanged;
        }

        private void EntityOnOnVariableChanged(BlackboardVariable<Entity> entityVariable)
        {
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

            var screenPoint = Camera.main.WorldToScreenPoint(entityTransform.position + worldOffset);
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = screenPoint;
        }
    }
}