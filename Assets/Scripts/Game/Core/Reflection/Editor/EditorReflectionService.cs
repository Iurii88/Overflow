using Game.Core.Logging;
using UnityEditor;

namespace Game.Core.Reflection.Editor
{
    [InitializeOnLoad]
    public class EditorReflectionService
    {
        private static ReflectionManager m_cachedInstance;

        static EditorReflectionService()
        {
            EditorApplication.delayCall += () => { GetOrCreateInstance(); };
        }

        public static IReflectionManager GetOrCreateInstance()
        {
            if (m_cachedInstance != null)
                return m_cachedInstance;

            m_cachedInstance = new ReflectionManager();
            m_cachedInstance.Initialize();

            GameLogger.Log("[EditorReflectionService] ReflectionManager initialized for editor use");
            return m_cachedInstance;
        }
    }
}