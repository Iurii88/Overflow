using Game.Core.Camera;
using Game.Features.Camera.Components;
using Game.Features.Pause;
using Game.Features.Pause.Groups;
using Unity.Mathematics;
using UnityEngine;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Camera.Systems
{
    [UpdateInGroup(typeof(PauseAwareSystemGroup))]
    public class CameraFollowingSystem : SystemBase
    {
        private EntityQuery m_cameraTargetQuery;

        [Inject]
        private ICameraManager m_cameraManager;

        [Inject]
        private IGameDeltaTime m_deltaTime;

        public override void OnAwake()
        {
            m_cameraTargetQuery = CreateQuery().With<CameraTarget>();
        }

        public override void OnUpdate()
        {
            var camera = m_cameraManager.MainCamera;
            if (camera == null)
                return;

            m_cameraTargetQuery.ForEach((ref Entity entity, ref CameraTarget cameraTarget) =>
            {
                var transform = entity.GetReference<Transform>();
                if (transform == null)
                    return;

                var targetPosition = (float3)transform.position + cameraTarget.offset;
                var smoothedPosition = math.lerp(camera.transform.position, targetPosition, cameraTarget.smoothSpeed * m_deltaTime.DeltaTime);
                camera.transform.position = smoothedPosition;
            });
        }
    }
}