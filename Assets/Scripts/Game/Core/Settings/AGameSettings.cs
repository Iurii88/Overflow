using System.Reflection;
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

#if UNITY_EDITOR
        public virtual bool DrawEditorField(FieldInfo field, object defaultInstance, bool isStandalonePreset)
        {
            return false;
        }
#endif
    }
}