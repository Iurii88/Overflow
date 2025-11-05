using Game.Core.Extensions.Filters;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.Content.Properties.Filters
{
    public class HasPropertyFilter<T> : IExtensionFilter where T : AContentProperty
    {
        public bool ShouldExecute(Entity entity, ContentEntity contentEntity)
        {
            return contentEntity != null && contentEntity.HasProperty<T>();
        }
    }
}