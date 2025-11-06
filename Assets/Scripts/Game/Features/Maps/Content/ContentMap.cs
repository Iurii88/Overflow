using Game.Core.Content;
using Game.Core.Content.Attributes;
using Unity.Mathematics;

namespace Game.Features.Maps.Content
{
    [ContentSchema("maps")]
    public class ContentMap : AContent
    {
        public string scene;
        public float2 playerSpawnPosition;
    }
}