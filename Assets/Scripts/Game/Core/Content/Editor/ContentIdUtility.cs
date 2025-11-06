using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Game.Core.Content.Attributes;
using Newtonsoft.Json.Linq;

namespace Game.Core.Content.Editor
{
    public static class ContentIdUtility
    {
        private static readonly Dictionary<Type, string[]> IDCache = new();

        public static string[] GetContentIds(Type contentType)
        {
            if (contentType == null)
                return Array.Empty<string>();

            if (IDCache.TryGetValue(contentType, out var cachedIds))
                return cachedIds;

            var schemaAttribute = contentType.GetCustomAttribute<ContentSchemaAttribute>();
            if (schemaAttribute == null)
                return Array.Empty<string>();

            var folderName = ContentManager.ContentFolderPrefix + schemaAttribute.schema;
            var contentFolders = Directory.GetDirectories(
                ContentManager.ContentRootPath,
                folderName,
                SearchOption.AllDirectories
            );

            if (contentFolders.Length == 0)
                return Array.Empty<string>();

            var ids = new List<string>();
            foreach (var folder in contentFolders)
            {
                var jsonFiles = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);
                foreach (var jsonFile in jsonFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(jsonFile);
                        var jObject = JObject.Parse(json);
                        if (!jObject.TryGetValue("id", out var idToken))
                            continue;

                        var id = idToken.ToString();
                        if (!string.IsNullOrEmpty(id))
                            ids.Add(id);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"[ContentIdUtility] Failed to read ID from {jsonFile}: {e.Message}");
                    }
                }
            }

            var idsArray = ids.OrderBy(id => id).ToArray();
            IDCache[contentType] = idsArray;
            return idsArray;
        }

        public static void ClearCache()
        {
            IDCache.Clear();
        }
    }
}