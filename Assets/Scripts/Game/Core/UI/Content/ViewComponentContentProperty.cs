using Game.Core.Content.Attributes;
using Game.Core.Content.Properties;

namespace Game.Core.UI.Content
{
    [Identifier("VIEW_COMPONENT")]
    public class ViewComponentContentProperty : AContentProperty
    {
        public string assetPath;
        public string layer;
        public bool enabled = true;
        public bool activeOnStart = true;
    }
}