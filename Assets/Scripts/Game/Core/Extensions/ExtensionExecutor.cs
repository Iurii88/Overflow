using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Features.Entities.Content;
using Game.Features.Sessions.Attributes;
using UnsafeEcs.Core.Entities;
using VContainer;

namespace Game.Core.Extensions
{
    [AutoRegister]
    public class ExtensionExecutor : IExtensionExecutor
    {
        [Inject]
        private readonly IObjectResolver m_resolver;

        private readonly Dictionary<Type, object> m_extensionCache = new Dictionary<Type, object>();

        public async UniTask ExecuteAsync<TExtension>
        (
            Entity entity,
            ContentEntity contentEntity,
            Func<TExtension, UniTask> action)
            where TExtension : IExtension
        {
            var extensions = GetOrResolveExtensions<TExtension>();

            for (var i = 0; i < extensions.Count; i++)
            {
                if (!ShouldExecute(extensions[i], entity, contentEntity))
                    continue;

                await action(extensions[i]);
            }
        }

        public void Execute<TExtension>(
            Entity entity,
            ContentEntity contentEntity,
            Action<TExtension> action)
            where TExtension : IExtension
        {
            var extensions = GetOrResolveExtensions<TExtension>();

            for (var i = 0; i < extensions.Count; i++)
            {
                if (!ShouldExecute(extensions[i], entity, contentEntity))
                    continue;

                action(extensions[i]);
            }
        }

        public async UniTask ExecuteAsync<TExtension>(Func<TExtension, UniTask> action)
            where TExtension : IExtension
        {
            var extensions = GetOrResolveExtensions<TExtension>();

            for (var i = 0; i < extensions.Count; i++)
            {
                await action(extensions[i]);
            }
        }

        public void Execute<TExtension>(Action<TExtension> action)
            where TExtension : IExtension
        {
            var extensions = GetOrResolveExtensions<TExtension>();

            for (var i = 0; i < extensions.Count; i++)
            {
                action(extensions[i]);
            }
        }

        private IReadOnlyList<TExtension> GetOrResolveExtensions<TExtension>()
            where TExtension : IExtension
        {
            var extensionType = typeof(TExtension);

            if (!m_extensionCache.TryGetValue(extensionType, out var cachedExtensions))
            {
                var resolvedExtensions = m_resolver.Resolve<IReadOnlyList<TExtension>>();
                var sortedExtensions = resolvedExtensions
                    .OrderBy(ext => GetExtensionPriority(ext))
                    .ToArray();

                m_extensionCache[extensionType] = sortedExtensions;
                cachedExtensions = sortedExtensions;
            }

            return (IReadOnlyList<TExtension>)cachedExtensions;
        }

        private static int GetExtensionPriority(IExtension extension)
        {
            var priorityAttribute = extension.GetType().GetCustomAttributes(typeof(ExtensionPriorityAttribute), false);
            if (priorityAttribute.Length > 0)
                return ((ExtensionPriorityAttribute)priorityAttribute[0]).Priority;

            return 0;
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

        public void Dispose()
        {
            m_extensionCache.Clear();
        }
    }
}