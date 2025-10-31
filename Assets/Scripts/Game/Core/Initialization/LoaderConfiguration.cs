using System;
using System.Collections.Generic;
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

            // Topological sort using Kahn's algorithm
            var queue = new Queue<IAsyncLoader>();
            var result = new List<IAsyncLoader>();

            // Start with nodes that have no dependencies
            foreach (var kvp in inDegree)
            {
                if (kvp.Value == 0)
                    queue.Enqueue(kvp.Key);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                // Reduce in-degree for dependent nodes
                foreach (var dependent in adjacencyList[current])
                {
                    inDegree[dependent]--;
                    if (inDegree[dependent] == 0)
                        queue.Enqueue(dependent);
                }
            }

            // Check for circular dependencies
            if (result.Count != m_loaders.Count)
            {
                var missing = m_loaders.AsValueEnumerable().Where(l => !result.Contains(l)).Select(l => l.GetType().Name);
                throw new InvalidOperationException(
                    $"Circular dependency detected among loaders: {string.Join(", ", missing)}"
                );
            }

            return result;
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
        }
    }
}