using Game.Core.Reflection;

namespace Game.Core.Settings
{
    public abstract class AGameSettings
    {
        public virtual void OnBeforeApply(IReflectionManager reflectionManager)
        {
        }

        public virtual void Apply()
        {
        }
    }
}