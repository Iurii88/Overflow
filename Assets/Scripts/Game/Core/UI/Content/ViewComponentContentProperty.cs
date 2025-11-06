using Game.Core.Content.Attributes;
using Game.Core.Content.Properties;
using Game.Core.UI.Layers;

namespace Game.Core.UI.Content
{
    [Identifier("VIEW_COMPONENT")]
    public class ViewComponentContentProperty : AContentProperty
    {
        public string assetPath;
        public UILayer layer;
    }
}
