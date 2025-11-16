using Unity.Mathematics;
using UnsafeEcs.Core.Components;

namespace Game.Features.Camera.Components
{
    public struct CameraTarget : IComponent
    {
        public float3 offset;
        public float smoothSpeed;
    }
}
