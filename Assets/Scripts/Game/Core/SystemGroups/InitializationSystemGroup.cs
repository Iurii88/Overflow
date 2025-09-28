using UnsafeEcs.Core.Bootstrap;
using UnsafeEcs.Core.Bootstrap.Attributes;
using UnsafeEcs.Core.Systems;

namespace Game.Core.SystemGroups
{
    [UpdateInWorld(WorldBootstrap.AllWorldsIndex)]
    public class InitializationSystemGroup : SystemGroup
    {
        
    }
}