using System.Collections.Generic;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.Extensions.Filters
{
    public class OrFilter : IExtensionFilter
    {
        private readonly IReadOnlyList<IExtensionFilter> m_filters;

        public OrFilter(params IExtensionFilter[] filters)
        {
            m_filters = filters;
        }

        public bool ShouldExecute(Entity entity, ContentEntity contentEntity)
        {
            for (var i = 0; i < m_filters.Count; i++)
            {
                if (m_filters[i].ShouldExecute(entity, contentEntity))
                    return true;
            }

            return false;
        }
    }
}