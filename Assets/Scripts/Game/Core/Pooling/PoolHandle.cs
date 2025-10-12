using System;
using Unity.Mathematics;

namespace Game.Core.Pooling
{
    public struct PoolHandle : IEquatable<PoolHandle>
    {
        public int index;
        public int generation;

        public static readonly PoolHandle Invalid = new() { index = -1, generation = -1 };

        public bool IsValid => index >= 0;

        public bool Equals(PoolHandle other)
        {
            return index == other.index && generation == other.generation;
        }

        public override int GetHashCode()
        {
            return (int)math.hash(new int2(index, generation));
        }
    }
}