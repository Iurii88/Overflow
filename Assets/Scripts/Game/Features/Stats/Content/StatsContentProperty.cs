using Game.Core.Content.Attributes;
using Game.Core.Content.Properties;
using Game.Features.Stats.Data;

namespace Game.Features.Stats.Content
{
    [Identifier("STATS")]
    public class StatsContentProperty : AContentProperty
    {
        public Stat[] stats;
    }
}
