using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Features.Entities.Content;
using UnsafeEcs.Core.Entities;
using VContainer;

namespace Game.Core.Extensions
{
    public static class ExtensionExecutor
    {
        private static readonly Dictionary<Type, object> ExtensionCache = new();

        public static async UniTask ExecuteAsync<TExtension>(
            IObjectResolver resolver,
            Entity entity,
            ContentEntity contentEntity,
            Func<TExtension, UniTask> action)
            where TExtension : IExtension
        {
            var extensions = GetOrResolveExtensions<TExtension>(resolver);

            for (var i = 0; i < extensions.Count; i++)
            {
                if (!ShouldExecute(extensions[i], entity, contentEntity))
                    continue;

                await action(extensions[i]);
            }
        }

        public static void Execute<TExtension>(
            IObjectResolver resolver,
            Entity entity,
            ContentEntity contentEntity,
            Action<TExtension> action)
            where TExtension : IExtension
        {
            var extensions = GetOrResolveExtensions<TExtension>(resolver);

            for (var i = 0; i < extensions.Count; i++)
            {
                if (!ShouldExecute(extensions[i], entity, contentEntity))
                    continue;

                action(extensions[i]);
            }
        }

        private static IReadOnlyList<TExtension> GetOrResolveExtensions<TExtension>(IObjectResolver resolver)
            where TExtension : IExtension
        {
            var extensionType = typeof(TExtension);

            if (!ExtensionCache.TryGetValue(extensionType, out var cachedExtensions))
            {
                cachedExtensions = resolver.Resolve<IReadOnlyList<TExtension>>();
                ExtensionCache[extensionType] = cachedExtensions;
            }

            return (IReadOnlyList<TExtension>)cachedExtensions;
        }

        private static bool ShouldExecute(IExtension extension, Entity entity, ContentEntity contentEntity)
        {
            if (extension is not IFilterableExtension filterableExtension)
                return true;

            var filters = filterableExtension.Filters;
            if (filters == null || filters.Count == 0)
                return true;

            for (var i = 0; i < filters.Count; i++)
            {
                if (!filters[i].ShouldExecute(entity, contentEntity))
                    return false;
            }

            return true;
        }
    }
}