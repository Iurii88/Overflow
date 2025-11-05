using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.Extensions.Filters
{
    public class NotFilter : IExtensionFilter
    {
        private readonly IExtensionFilter m_filter;

        public NotFilter(IExtensionFilter filter)
        {
            m_filter = filter;
        }

        public bool ShouldExecute(Entity entity, ContentEntity contentEntity)
        {
            return !m_filter.ShouldExecute(entity, contentEntity);
        }
    }
}
