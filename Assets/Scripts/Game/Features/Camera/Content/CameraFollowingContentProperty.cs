using Game.Core.Content.Attributes;
using Game.Core.Content.Properties;
using Unity.Mathematics;

namespace Game.Features.Camera.Content
{
    [Identifier("CAMERA_FOLLOWING")]
    public class CameraFollowingContentProperty : AContentProperty
    {
        public float3 offset = new(0, 0, -10);
        public float smoothSpeed = 5f;
    }
}
