using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.Extensions.Filters
{
    public interface IExtensionFilter
    {
        bool ShouldExecute(Entity entity, ContentEntity contentEntity);
    }
}
