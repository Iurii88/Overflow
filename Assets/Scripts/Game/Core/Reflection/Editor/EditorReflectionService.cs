using UnityEditor;
using UnityEngine;

namespace Game.Core.Reflection.Editor
{
    /// <summary>
    ///     Provides access to ReflectionManager for editor windows and tools.
    ///     Editor windows should request an instance via GetOrCreateInstance() and then pass it as a dependency.
    /// </summary>
    [InitializeOnLoad]
    public class EditorReflectionService
    {
        private const string SessionStateKey = "EditorReflectionService.Instance";

        private static ReflectionManager m_cachedInstance;

        static EditorReflectionService()
        {
            EditorApplication.delayCall += () =>
            {
                // Ensure instance is created on editor startup
                GetOrCreateInstance();
            };
        }

        /// <summary>
        ///     Gets or creates a ReflectionManager instance for editor use.
        ///     Editor windows should call this once and pass the instance as a dependency.
        /// </summary>
        public static IReflectionManager GetOrCreateInstance()
        {
            if (m_cachedInstance != null)
                return m_cachedInstance;
            
            m_cachedInstance = new ReflectionManager();
            m_cachedInstance.Initialize();

            Debug.Log("[EditorReflectionService] ReflectionManager initialized for editor use");

            return m_cachedInstance;
        }

        /// <summary>
        ///     Creates a new ReflectionManager instance. Use this if you need a separate instance.
        /// </summary>
        public static IReflectionManager CreateNewInstance()
        {
            var instance = new ReflectionManager();
            instance.Initialize();
            return instance;
        }
    }
}