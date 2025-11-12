using System;
using System.Runtime.CompilerServices;

namespace Game.Core.Pooling
{
    public readonly struct PoolKey : IEquatable<PoolKey>
    {
        private readonly int typeHash;
        private readonly int keyHash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PoolKey(Type type, string key)
        {
            typeHash = type.GetHashCode();
            keyHash = key.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PoolKey other)
        {
            return typeHash == other.typeHash && keyHash == other.keyHash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            unchecked
            {
                return (typeHash * 397) ^ keyHash;
            }
        }
    }
}