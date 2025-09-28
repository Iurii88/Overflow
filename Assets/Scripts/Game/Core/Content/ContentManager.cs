using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Content.Attributes;
using Game.Core.Content.Converters.Registry;
using Game.Core.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using VContainer;

namespace Game.Core.Content
{
    public class ContentManager : IContentManager
    {
        [Inject]
        private IReflectionManager m_reflectionManager;

        [Inject]
        private JsonConverterRegistry m_jsonConvertersRegistry;

        private readonly Dictionary<Type, Dictionary<string, object>> m_contentCache = new();
        private readonly Dictionary<string, Type> m_schemaTypeMapping = new();
        private readonly Dictionary<Type, Array> m_getAllCache = new();

        private bool m_isInitialized;

        private const string ContentRootPath = "Assets/GameAssets";
        private const string ContentFolderPrefix = "#";
        private const string IDFieldName = "id";

        public async UniTask StartAsync(CancellationToken cancellationToken = new())
        {
            if (m_isInitialized)
                return;

            await UniTask.SwitchToThreadPool();

            RegisterContentSchemas();
            await LoadAllContentAsync(cancellationToken);
            m_isInitialized = true;

            await UniTask.SwitchToMainThread();
        }

        private void RegisterContentSchemas()
        {
            var types = m_reflectionManager.GetTypesWithAttribute<ContentSchemaAttribute>();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ContentSchemaAttribute>();
                if (attribute == null)
                    continue;

                m_schemaTypeMapping[ContentFolderPrefix + attribute.schema] = type;
                m_contentCache[type] = new Dictionary<string, object>();

                Debug.Log($"[ContentManager] Registered schema: {attribute.schema} -> {type.Name}");
            }
        }

        private async UniTask LoadAllContentAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(ContentRootPath))
            {
                Debug.LogWarning($"[ContentManager] Content root path does not exist: {ContentRootPath}");
                return;
            }

            var searchPattern = $"{ContentFolderPrefix}*";
            var contentFolders = Directory.GetDirectories(
                ContentRootPath,
                searchPattern,
                SearchOption.AllDirectories
            );

            if (contentFolders.Length == 0)
            {
                Debug.LogWarning($"[ContentManager] No content folders found with pattern '{searchPattern}' in: {ContentRootPath}");
                return;
            }

            Debug.Log($"[ContentManager] Found {contentFolders.Length} content folders to load");

            var loadTasks = new List<UniTask>(contentFolders.Length);
            foreach (var folder in contentFolders)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("[ContentManager] Loading cancelled");
                    return;
                }

                loadTasks.Add(LoadContentFromFolderAsync(folder, cancellationToken));
            }

            try
            {
                await UniTask.WhenAll(loadTasks);
                Debug.Log($"[ContentManager] Successfully loaded content from {loadTasks.Count} folders");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[ContentManager] Content loading was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ContentManager] Error during content loading: {ex.Message}");
                throw;
            }
        }

        private async UniTask LoadContentFromFolderAsync(string folderPath, CancellationToken cancellationToken)
        {
            var folderName = Path.GetFileName(folderPath);
            if (!m_schemaTypeMapping.TryGetValue(folderName, out var contentType))
            {
                Debug.LogWarning($"[ContentManager] No schema registered for folder: {folderName}");
                return;
            }

            var jsonFiles = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);
            foreach (var jsonFile in jsonFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await LoadJsonFileAsync(jsonFile, contentType);
            }
        }

        private async UniTask LoadJsonFileAsync(string filePath, Type contentType)
        {
            try
            {
                var json = await ReadFileAsync(filePath);
                if (string.IsNullOrEmpty(json))
                    return;

                var content = JsonConvert.DeserializeObject(json, contentType, m_jsonConvertersRegistry.GetSettings());
                if (content == null)
                    return;

                var idField = contentType.GetField(IDFieldName);
                if (idField == null)
                {
                    Debug.LogError($"[ContentManager] Content type {contentType.Name} must have an 'id' property");
                    return;
                }

                var id = idField.GetValue(content) as string;
                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogError($"[ContentManager] Content in file {filePath} has no valid id");
                    return;
                }

                m_contentCache[contentType][id] = content;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ContentManager] Failed to load {filePath}: {e}");
            }
        }

        private static async UniTask<string> ReadFileAsync(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                return await reader.ReadToEndAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ContentManager] Error reading file {filePath}: {e}");
                return null;
            }
        }

        public T Get<T>(string id) where T : class
        {
            var type = typeof(T);
            if (!m_contentCache.TryGetValue(type, out var typeCache))
            {
                Debug.LogError($"[ContentManager] No content registered for type: {type.Name}");
                return null;
            }

            if (!typeCache.TryGetValue(id, out var content))
            {
                Debug.LogWarning($"[ContentManager] Content not found: {id} of type {type.Name}");
                return null;
            }

            return content as T;
        }

        public T[] GetAll<T>() where T : class
        {
            var type = typeof(T);
            if (m_getAllCache.TryGetValue(type, out var cachedArray))
                return (T[])cachedArray;

            if (!m_contentCache.TryGetValue(type, out var typeCache))
                return Array.Empty<T>();

            var array = typeCache.Values.Cast<T>().ToArray();
            m_getAllCache[type] = array;
            return array;
        }

        public void Dispose()
        {
            m_contentCache.Clear();
            m_schemaTypeMapping.Clear();
            m_getAllCache.Clear();
        }
    }
}