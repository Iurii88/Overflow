using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Game.Core.Content.Editor
{
    public class ContentIndexGenerator : AssetPostprocessor
    {
        [MenuItem("Tools/Content/Generate Content Index")]
        public static void GenerateContentIndex()
        {
            try
            {
                var index = BuildContentIndex();
                SaveContentIndex(index);
                EnsureContentIndexInAddressables();
                AssetDatabase.Refresh();

                Debug.Log($"[ContentIndexGenerator] Successfully generated content index with {index.entries.Count} entries");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ContentIndexGenerator] Failed to generate content index: {e}");
            }
        }

        private static ContentIndex BuildContentIndex()
        {
            var index = new ContentIndex();
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[ContentIndexGenerator] Addressable Asset Settings not found");
                return index;
            }

            var contentGroup = GetOrCreateContentGroup(settings);
            if (contentGroup == null)
            {
                Debug.LogError("[ContentIndexGenerator] Failed to get or create content group");
                return index;
            }

            if (!Directory.Exists(ContentConstants.ContentRootPath))
            {
                Debug.LogWarning($"[ContentIndexGenerator] Content root path does not exist: {ContentConstants.ContentRootPath}");
                return index;
            }

            var contentFolders = Directory.GetDirectories(ContentConstants.ContentRootPath, ContentConstants.ContentFolderPrefix + "*", SearchOption.AllDirectories);

            foreach (var folder in contentFolders)
            {
                var schema = Path.GetFileName(folder).Substring(1);
                var jsonFiles = Directory.GetFiles(folder, "*.json", SearchOption.AllDirectories);

                foreach (var jsonFile in jsonFiles)
                {
                    try
                    {
                        var contentId = ExtractContentId(jsonFile);
                        if (string.IsNullOrEmpty(contentId))
                            continue;

                        var addressablePath = $"Content/{schema}/{contentId}";

                        AddToAddressables(settings, contentGroup, jsonFile, addressablePath);

                        index.entries.Add(new ContentIndexEntry
                        {
                            schema = schema,
                            id = contentId,
                            addressablePath = addressablePath
                        });

                        Debug.Log($"[ContentIndexGenerator] Added content: {contentId} ({schema}) -> {addressablePath}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ContentIndexGenerator] Failed to process {jsonFile}: {e}");
                    }
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);

            return index;
        }

        private static string ExtractContentId(string jsonFilePath)
        {
            try
            {
                var json = File.ReadAllText(jsonFilePath);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (data != null && data.TryGetValue("id", out var idObj))
                    return idObj?.ToString();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ContentIndexGenerator] Failed to extract ID from {jsonFilePath}: {e}");
            }

            return null;
        }

        private static void SaveContentIndex(ContentIndex index)
        {
            var json = JsonConvert.SerializeObject(index, Formatting.Indented);
            File.WriteAllText(ContentConstants.ContentIndexPath, json);
            AssetDatabase.ImportAsset(ContentConstants.ContentIndexPath);
        }

        private static void EnsureContentIndexInAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            var contentGroup = GetOrCreateContentGroup(settings);
            if (contentGroup == null)
                return;

            AddToAddressables(settings, contentGroup, ContentConstants.ContentIndexPath, ContentConstants.ContentIndexAddressablePath);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        }

        private static AddressableAssetGroup GetOrCreateContentGroup(AddressableAssetSettings settings)
        {
            var group = settings.FindGroup(ContentConstants.ContentGroupName);
            if (group != null)
                return group;

            group = settings.CreateGroup(ContentConstants.ContentGroupName, false, false, true, null, typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema),
                typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema));
            Debug.Log($"[ContentIndexGenerator] Created addressables group: {ContentConstants.ContentGroupName}");

            return group;
        }

        private static void AddToAddressables(AddressableAssetSettings settings, AddressableAssetGroup group, string assetPath, string addressablePath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"[ContentIndexGenerator] Could not find GUID for asset: {assetPath}");
                return;
            }

            var entry = settings.FindAssetEntry(guid);
            if (entry != null)
            {
                if (entry.address != addressablePath)
                {
                    entry.SetAddress(addressablePath);
                    Debug.Log($"[ContentIndexGenerator] Updated addressable path: {addressablePath}");
                }

                return;
            }

            entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.SetAddress(addressablePath);
            Debug.Log($"[ContentIndexGenerator] Added to addressables: {addressablePath}");
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var contentChanged = false;

            foreach (var asset in importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths))
            {
                if (!asset.StartsWith(ContentConstants.ContentRootPath) || !asset.EndsWith(".json") || asset.EndsWith(ContentConstants.ContentIndexFileName))
                    continue;

                contentChanged = true;
                break;
            }

            if (!contentChanged)
                return;

            Debug.Log("[ContentIndexGenerator] Content files changed, regenerating content index...");
            GenerateContentIndex();
        }
    }
}