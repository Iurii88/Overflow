using Game.Core.Content.Attributes;
using Game.Core.Content.Properties;

namespace Game.Features.Movement.Content
{
    [Identifier("MOVABLE")]
    public class MovableContentProperty : AContentProperty
    {
        public float baseSpeed;
    }
}