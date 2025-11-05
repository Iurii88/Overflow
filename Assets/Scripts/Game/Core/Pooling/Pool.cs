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
        private int[] m_denseToSparse;

        public int Count { get; private set; }
        public int Capacity { get; private set; }

        public Pool(Func<T> createFunc, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, int initialCapacity = 32, int maxPoolSize = 1000)
        {
            var func = createFunc ?? throw new ArgumentNullException(nameof(createFunc));

            Capacity = initialCapacity;
            m_dense = new T[Capacity];
            m_sparse = new int[Capacity];
            m_denseToSparse = new int[Capacity];
            m_generations = new int[Capacity];
            m_freeIndices = new Queue<int>();

            for (var i = 0; i < Capacity; i++)
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
                index = Count;

                if (index >= Capacity)
                    Resize(Capacity * 2);
            }

            var denseIndex = Count;
            m_dense[denseIndex] = obj;
            m_sparse[index] = denseIndex;
            m_denseToSparse[denseIndex] = index;
            Count++;

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
                denseIndex < Count &&
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

            var lastIndex = Count - 1;
            if (denseIndex != lastIndex)
            {
                var lastSparseIndex = m_denseToSparse[lastIndex];
                m_dense[denseIndex] = m_dense[lastIndex];
                m_sparse[lastSparseIndex] = denseIndex;
                m_denseToSparse[denseIndex] = lastSparseIndex;
            }

            m_dense[lastIndex] = null;
            m_sparse[sparseIndex] = -1;
            Count--;

            m_generations[sparseIndex]++;
            m_freeIndices.Enqueue(sparseIndex);

            m_pool.Release(obj);

            return true;
        }

        public void Clear()
        {
            for (var i = 0; i < Count; i++)
                if (m_dense[i] != null)
                {
                    m_pool.Release(m_dense[i]);
                    m_dense[i] = null;
                }

            Count = 0;
            m_freeIndices.Clear();

            for (var i = 0; i < Capacity; i++)
                m_sparse[i] = -1;
        }

        private void Resize(int newCapacity)
        {
            Array.Resize(ref m_dense, newCapacity);
            Array.Resize(ref m_sparse, newCapacity);
            Array.Resize(ref m_denseToSparse, newCapacity);
            Array.Resize(ref m_generations, newCapacity);

            for (var i = Capacity; i < newCapacity; i++)
            {
                m_sparse[i] = -1;
                m_generations[i] = 0;
            }

            Capacity = newCapacity;
        }
    }
}