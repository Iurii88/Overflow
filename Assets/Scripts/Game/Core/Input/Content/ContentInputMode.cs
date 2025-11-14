using System.Collections.Generic;
using Game.Core.Content;
using Game.Core.Content.Attributes;

namespace Game.Core.Input.Content
{
    [ContentSchema("inputmodes")]
    public class ContentInputMode : AContent
    {
        public List<string> actionMaps = new();
    }
}