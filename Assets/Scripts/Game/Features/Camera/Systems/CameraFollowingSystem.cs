using Game.Core.Camera;
using Game.Features.Camera.Components;
using Game.Features.Pause;
using Game.Features.Pause.Groups;
using Unity.Mathematics;
using UnityEngine;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Components.Managed;
using UnsafeEcs.Core.Entities;
using UnsafeEcs.Core.Systems;
using VContainer;

namespace Game.Features.Camera.Systems
{
    [UpdateInGroup(typeof(TimeSystemGroup))]
    public class CameraFollowingSystem : SystemBase
    {
        private EntityQuery m_cameraTargetQuery;

        [Inject]
        private ICameraManager m_cameraManager;

        [Inject]
        private ISessionTime time;

        public override void OnAwake()
        {
            m_cameraTargetQuery = CreateQuery().With<CameraTarget>().With<ManagedRef<Transform>>();
        }

        public override void OnUpdate()
        {
            var camera = m_cameraManager.MainCamera;
            if (camera == null)
                return;

            m_cameraTargetQuery.ForEach((time.DeltaTime, camera),
                ((float deltaTime, UnityEngine.Camera cam) context,
                    ref Entity _, ref CameraTarget cameraTarget, ref ManagedRef<Transform> transformRef) =>
                {
                    var transform = transformRef.Get();
                    var targetPosition = (float3)transform.position + cameraTarget.offset;
                    var smoothedPosition = math.lerp(context.cam.transform.position, targetPosition, cameraTarget.smoothSpeed * context.deltaTime);
                    context.cam.transform.position = smoothedPosition;
                });
        }
    }
}