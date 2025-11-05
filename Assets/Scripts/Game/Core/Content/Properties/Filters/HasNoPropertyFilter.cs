using Game.Core.Extensions.Filters;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.Content.Properties.Filters
{
    public class HasNoPropertyFilter<T> : IExtensionFilter where T : AContentProperty
    {
        public bool ShouldExecute(Entity entity, ContentEntity contentEntity)
        {
            if (contentEntity == null)
                return false;

            return !contentEntity.HasProperty<T>();
        }
    }
}