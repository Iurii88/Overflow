using Game.Core.Content.Properties;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;

namespace Game.Core.Content.Utils
{
    public static class ContentExtensions
    {
        public static ContentEntity GetContent(this Entity entity)
        {
            return entity.GetReference<ContentEntity>();
        }
    }
}