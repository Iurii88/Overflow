using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core.Addressables;
using Game.Core.Content.Attributes;
using Game.Core.Content.Converters.Registry;
using Game.Core.Logging;
using Game.Core.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZLinq;

namespace Game.Core.Content
{
    public class ContentManager : IContentManager
    {
        [Inject]
        private IReflectionManager m_reflectionManager;

        [Inject]
        private JsonConverterRegistry m_jsonConvertersRegistry;

        [Inject]
        private IAddressableManager m_addressableManager;

        private readonly Dictionary<Type, Dictionary<string, object>> m_contentCache = new();
        private readonly Dictionary<string, Type> m_schemaTypeMapping = new();
        private readonly Dictionary<Type, Array> m_getAllCache = new();

        public bool IsInitialized { get; private set; }

        public async UniTask LoadAsync(CancellationToken cancellation = new())
        {
            if (IsInitialized)
                return;

            RegisterContentSchemas();
            await LoadAllContentAsync(cancellation);

            IsInitialized = true;
        }

        private void RegisterContentSchemas()
        {
            var types = m_reflectionManager.GetByAttribute<ContentSchemaAttribute>();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ContentSchemaAttribute>();
                if (attribute == null)
                    continue;

                m_schemaTypeMapping[ContentConstants.ContentFolderPrefix + attribute.schema] = type;
                m_contentCache[type] = new Dictionary<string, object>();

                GameLogger.Log($"[ContentManager] Registered schema: {attribute.schema} -> {type.Name}");
            }
        }

        private async UniTask LoadAllContentAsync(CancellationToken cancellationToken)
        {
            try
            {
                var indexAsset = await m_addressableManager.LoadAssetAsync<TextAsset>(ContentConstants.ContentIndexAddressablePath, cancellationToken);
                if (indexAsset == null)
                {
                    GameLogger.Error("[ContentManager] Failed to load ContentIndex from addressables");
                    return;
                }

                var contentIndex = JsonConvert.DeserializeObject<ContentIndex>(indexAsset.text);
                if (contentIndex == null || contentIndex.entries.Count == 0)
                {
                    GameLogger.Warning("[ContentManager] ContentIndex is empty or invalid");
                    return;
                }

                GameLogger.Log($"[ContentManager] Loading {contentIndex.entries.Count} content entries from index");

                var loadTasks = new List<UniTask>(contentIndex.entries.Count);
                foreach (var entry in contentIndex.entries)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        GameLogger.Log("[ContentManager] Loading cancelled");
                        return;
                    }

                    loadTasks.Add(LoadContentFromIndexAsync(entry, cancellationToken));
                }

                await UniTask.WhenAll(loadTasks);
                GameLogger.Log($"[ContentManager] Successfully loaded {contentIndex.entries.Count} content entries");
            }
            catch (OperationCanceledException)
            {
                GameLogger.Log("[ContentManager] Content loading was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                GameLogger.Error($"[ContentManager] Error during content loading: {ex.Message}");
                throw;
            }
        }

        private async UniTask LoadContentFromIndexAsync(ContentIndexEntry entry, CancellationToken cancellationToken)
        {
            var schemaKey = ContentConstants.ContentFolderPrefix + entry.schema;
            if (!m_schemaTypeMapping.TryGetValue(schemaKey, out var contentType))
            {
                GameLogger.Warning($"[ContentManager] No schema registered for: {entry.schema}");
                return;
            }

            try
            {
                var textAsset = await m_addressableManager.LoadAssetAsync<TextAsset>(entry.addressablePath, cancellationToken);
                if (textAsset == null)
                {
                    GameLogger.Error($"[ContentManager] Failed to load content asset: {entry.addressablePath}");
                    return;
                }

                var content = JsonConvert.DeserializeObject(textAsset.text, contentType, m_jsonConvertersRegistry.GetSettings());
                if (content == null)
                {
                    GameLogger.Error($"[ContentManager] Failed to deserialize content: {entry.id}");
                    return;
                }

                var idField = contentType.GetField(ContentConstants.IDFieldName);
                if (idField == null)
                {
                    GameLogger.Error($"[ContentManager] Content type {contentType.Name} must have an 'id' property");
                    return;
                }

                var id = idField.GetValue(content) as string;
                if (string.IsNullOrEmpty(id))
                {
                    GameLogger.Error($"[ContentManager] Content {entry.addressablePath} has no valid id");
                    return;
                }

                if (id != entry.id)
                {
                    GameLogger.Warning($"[ContentManager] Content ID mismatch: index={entry.id}, content={id}");
                }

                m_contentCache[contentType][id] = content;

                if (content is IInitializable initializable)
                    initializable.Initialize();

                GameLogger.Log($"[ContentManager] Loaded content: {id} ({entry.schema})");
            }
            catch (Exception e)
            {
                GameLogger.Error($"[ContentManager] Failed to load content {entry.id}: {e}");
            }
        }

        public T Get<T>(string id) where T : class
        {
            var type = typeof(T);
            if (!m_contentCache.TryGetValue(type, out var typeCache))
            {
                GameLogger.Error($"[ContentManager] No content registered for type: {type.Name}");
                return null;
            }

            if (!typeCache.TryGetValue(id, out var content))
            {
                GameLogger.Error($"[ContentManager] Content not found: {id} of type {type.Name}");
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

            var array = typeCache.Values.AsValueEnumerable().Cast<T>().ToArray();
            m_getAllCache[type] = array;
            return array;
        }

        public void Dispose()
        {
            m_contentCache.Clear();
            m_schemaTypeMapping.Clear();
            m_getAllCache.Clear();
            IsInitialized = false;
        }
    }
}