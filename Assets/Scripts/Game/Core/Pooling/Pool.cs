using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Game.Core.Pooling
{
    public class Pool<T> where T : class
    {
        private readonly Queue<int> m_freeIndices;
        private readonly ObjectPool<T> m_pool;

        private T[] m_dense;
        private int[] m_generations;
        private int[] m_sparse;

        public int count { get; private set; }
        public int capacity { get; private set; }

        public Pool(Func<T> createFunc, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, int initialCapacity = 32, int maxPoolSize = 1000)
        {
            var func = createFunc ?? throw new ArgumentNullException(nameof(createFunc));

            capacity = initialCapacity;
            m_dense = new T[capacity];
            m_sparse = new int[capacity];
            m_generations = new int[capacity];
            m_freeIndices = new Queue<int>();

            for (var i = 0; i < capacity; i++)
            {
                m_sparse[i] = -1;
                m_generations[i] = 0;
            }

            m_pool = new ObjectPool<T>(func, onGet, onRelease, onDestroy, true, initialCapacity, maxPoolSize);
        }

        public PoolHandle Rent(out T obj)
        {
            obj = m_pool.Get();

            if (!m_freeIndices.TryDequeue(out var index))
            {
                index = count;

                if (index >= capacity)
                    Resize(capacity * 2);
            }

            var denseIndex = count;
            m_dense[denseIndex] = obj;
            m_sparse[index] = denseIndex;
            count++;

            var handle = new PoolHandle
            {
                index = index,
                generation = m_generations[index]
            };

            return handle;
        }

        public bool TryGet(PoolHandle handle, out T obj)
        {
            if (!handle.IsValid || handle.index >= m_sparse.Length)
            {
                obj = null;
                return false;
            }

            var denseIndex = m_sparse[handle.index];

            if (denseIndex >= 0 &&
                denseIndex < count &&
                m_generations[handle.index] == handle.generation)
            {
                obj = m_dense[denseIndex];
                return true;
            }

            obj = null;
            return false;
        }

        public T GetUnsafe(PoolHandle handle)
        {
            return m_dense[m_sparse[handle.index]];
        }

        public bool Return(PoolHandle handle)
        {
            if (!TryGet(handle, out var obj))
                return false;

            var sparseIndex = handle.index;
            var denseIndex = m_sparse[sparseIndex];

            var lastIndex = count - 1;
            if (denseIndex != lastIndex)
            {
                m_dense[denseIndex] = m_dense[lastIndex];

                for (var i = 0; i < capacity; i++)
                    if (m_sparse[i] == lastIndex)
                    {
                        m_sparse[i] = denseIndex;
                        break;
                    }
            }

            m_dense[lastIndex] = null;
            m_sparse[sparseIndex] = -1;
            count--;

            m_generations[sparseIndex]++;
            m_freeIndices.Enqueue(sparseIndex);

            m_pool.Release(obj);

            return true;
        }

        public void Clear()
        {
            for (var i = 0; i < count; i++)
                if (m_dense[i] != null)
                {
                    m_pool.Release(m_dense[i]);
                    m_dense[i] = null;
                }

            count = 0;
            m_freeIndices.Clear();

            for (var i = 0; i < capacity; i++)
                m_sparse[i] = -1;
        }

        private void Resize(int newCapacity)
        {
            Array.Resize(ref m_dense, newCapacity);
            Array.Resize(ref m_sparse, newCapacity);
            Array.Resize(ref m_generations, newCapacity);

            for (var i = capacity; i < newCapacity; i++)
            {
                m_sparse[i] = -1;
                m_generations[i] = 0;
            }

            capacity = newCapacity;
        }
    }
}