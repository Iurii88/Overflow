using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ZLinq;

namespace Game.Core.Initialization
{
    /// <summary>
    ///     Fluent API for configuring loader dependencies and execution order.
    /// </summary>
    public class LoaderConfiguration
    {
        private readonly List<IAsyncLoader> m_loaders = new();
        private readonly Dictionary<IAsyncLoader, List<IAsyncLoader>> m_dependencies = new();

        /// <summary>
        ///     Registers a loader with no dependencies.
        /// </summary>
        public LoaderRegistration Register(IAsyncLoader loader)
        {
            if (m_loaders.Contains(loader))
                return new LoaderRegistration(this, loader);

            m_loaders.Add(loader);
            m_dependencies[loader] = new List<IAsyncLoader>();

            return new LoaderRegistration(this, loader);
        }

        /// <summary>
        ///     Resolves and returns loaders in dependency order using topological sort.
        /// </summary>
        public List<IAsyncLoader> ResolveOrder()
        {
            var levels = ResolveLevels();
            var result = new List<IAsyncLoader>();

            foreach (var level in levels)
            {
                result.AddRange(level);
            }

            return result;
        }

        /// <summary>
        ///     Resolves loaders into parallel execution levels.
        ///     Each level contains loaders that can execute in parallel.
        ///     Loaders in level N depend only on loaders from levels 0 to N-1.
        /// </summary>
        public List<List<IAsyncLoader>> ResolveLevels()
        {
            var inDegree = new Dictionary<IAsyncLoader, int>();
            var adjacencyList = new Dictionary<IAsyncLoader, List<IAsyncLoader>>();

            // Initialize graph
            foreach (var loader in m_loaders)
            {
                inDegree[loader] = 0;
                adjacencyList[loader] = new List<IAsyncLoader>();
            }

            // Build dependency graph
            foreach (var kvp in m_dependencies)
            {
                var dependent = kvp.Key;
                var dependencies = kvp.Value;

                foreach (var dependency in dependencies)
                {
                    // Add edge: dependency -> dependent
                    adjacencyList[dependency].Add(dependent);
                    inDegree[dependent]++;
                }
            }

            // Modified Kahn's algorithm to group loaders by dependency level
            var levels = new List<List<IAsyncLoader>>();
            var currentLevel = new List<IAsyncLoader>();
            var processedCount = 0;

            // Start with nodes that have no dependencies (level 0)
            foreach (var kvp in inDegree)
            {
                if (kvp.Value == 0)
                    currentLevel.Add(kvp.Key);
            }

            while (currentLevel.Count > 0)
            {
                levels.Add(new List<IAsyncLoader>(currentLevel));
                processedCount += currentLevel.Count;

                var nextLevel = new List<IAsyncLoader>();

                // Process all loaders in current level
                foreach (var current in currentLevel)
                {
                    // Reduce in-degree for dependent nodes
                    foreach (var dependent in adjacencyList[current])
                    {
                        inDegree[dependent]--;
                        if (inDegree[dependent] == 0)
                            nextLevel.Add(dependent);
                    }
                }

                currentLevel = nextLevel;
            }

            // Check for circular dependencies
            if (processedCount != m_loaders.Count)
            {
                var missing = m_loaders.AsValueEnumerable().Where(l =>
                {
                    foreach (var level in levels)
                    {
                        if (level.Contains(l))
                            return false;
                    }
                    return true;
                }).Select(l => l.GetType().Name);

                throw new InvalidOperationException(
                    $"Circular dependency detected among loaders: {string.Join(", ", missing)}"
                );
            }

            return levels;
        }

        /// <summary>
        ///     Loads all registered loaders in parallel with dependency awareness.
        ///     Loaders at the same dependency level execute in parallel.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <param name="onProgress">Progress callback (progress 0-1, current loader name, completed count, total count)</param>
        public async UniTask LoadAsync(CancellationToken cancellationToken, Action<float, string, int, int> onProgress = null)
        {
            var levels = ResolveLevels();
            var totalLoaders = m_loaders.Count;
            var completedLoaders = 0;

            for (var levelIndex = 0; levelIndex < levels.Count; levelIndex++)
            {
                var level = levels[levelIndex];
                var loadTasks = new List<UniTask>();

                // Start all loaders in this level in parallel
                foreach (var loader in level)
                {
                    var loaderName = loader.GetType().Name;
                    loadTasks.Add(LoadWithProgress(loader, cancellationToken, () =>
                    {
                        completedLoaders++;
                        var progress = (float)completedLoaders / totalLoaders;
                        onProgress?.Invoke(progress, loaderName, completedLoaders, totalLoaders);
                    }));
                }

                // Wait for all loaders in this level to complete
                await UniTask.WhenAll(loadTasks);
            }
        }

        private static async UniTask LoadWithProgress(IAsyncLoader loader, CancellationToken cancellationToken, Action onComplete)
        {
            await loader.LoadAsync(cancellationToken);
            onComplete?.Invoke();
        }

        /// <summary>
        ///     Adds a dependency for a loader (internal use by LoaderRegistration).
        /// </summary>
        private void AddDependency(IAsyncLoader dependent, IAsyncLoader dependency)
        {
            if (!m_loaders.Contains(dependency))
            {
                throw new InvalidOperationException(
                    $"Cannot add dependency on unregistered loader: {dependency.GetType().Name}. Register it first."
                );
            }

            if (!m_dependencies[dependent].Contains(dependency))
            {
                m_dependencies[dependent].Add(dependency);
            }
        }

        /// <summary>
        ///     Fluent registration API for configuring loader dependencies.
        /// </summary>
        public class LoaderRegistration
        {
            private readonly LoaderConfiguration m_configuration;
            private readonly IAsyncLoader m_loader;

            internal LoaderRegistration(LoaderConfiguration configuration, IAsyncLoader loader)
            {
                m_configuration = configuration;
                m_loader = loader;
            }

            /// <summary>
            ///     Specifies that this loader should execute after another loader.
            /// </summary>
            public LoaderRegistration After(IAsyncLoader dependency)
            {
                m_configuration.AddDependency(m_loader, dependency);
                return this;
            }

            /// <summary>
            ///     Specifies that this loader should execute after multiple loaders.
            /// </summary>
            public LoaderRegistration After(params IAsyncLoader[] dependencies)
            {
                foreach (var dependency in dependencies)
                {
                    m_configuration.AddDependency(m_loader, dependency);
                }

                return this;
            }

            /// <summary>
            ///     Specifies that another loader should execute after this one.
            /// </summary>
            public LoaderRegistration Before(IAsyncLoader dependent)
            {
                m_configuration.Register(dependent).After(m_loader);
                return this;
            }

            /// <summary>
            ///     Registers another loader and returns its registration for chaining.
            /// </summary>
            public LoaderRegistration Register(IAsyncLoader loader)
            {
                return m_configuration.Register(loader);
            }

            /// <summary>
            ///     Resolves and returns loaders in dependency order.
            /// </summary>
            public List<IAsyncLoader> ResolveOrder()
            {
                return m_configuration.ResolveOrder();
            }

            /// <summary>
            ///     Resolves loaders into parallel execution levels.
            /// </summary>
            public List<List<IAsyncLoader>> ResolveLevels()
            {
                return m_configuration.ResolveLevels();
            }

            /// <summary>
            ///     Loads all registered loaders in parallel with dependency awareness.
            /// </summary>
            /// <param name="cancellationToken">Cancellation token for the operation</param>
            /// <param name="onProgress">Progress callback (progress 0-1, current loader name, completed count, total count)</param>
            public UniTask LoadAsync(CancellationToken cancellationToken, Action<float, string, int, int> onProgress = null)
            {
                return m_configuration.LoadAsync(cancellationToken, onProgress);
            }
        }
    }
}