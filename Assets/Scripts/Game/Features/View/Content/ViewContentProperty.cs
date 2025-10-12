using Game.Core.Content.Attributes;
using Game.Core.Content.Properties;

namespace Game.Features.Entities.Content
{
    [Identifier("VIEW")]
    public class ViewContentProperty : AContentProperty
    {
        public string assetPath;
    }
}