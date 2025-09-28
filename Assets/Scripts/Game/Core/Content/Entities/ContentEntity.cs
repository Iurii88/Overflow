using Game.Core.Content.Abstacts;
using Game.Core.Content.Attributes;
using Game.Core.Content.Properties;

namespace Game.Core.Content.Entities
{
    [ContentSchema("entities")]
    public class ContentEntity : AContent
    {
        public ContentProperty[] properties;
    }
}