using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Game.Core.Content.Attributes;
using Game.Core.Reflection.Attributes;
using UnityEngine;
using VContainer.Unity;
using ZLinq;

namespace Game.Core.Reflection
{
    public sealed class ReflectionManager : IReflectionManager, IInitializable
    {
        private readonly Dictionary<Type, TypeInfo[]> m_derivedTypesCache = new();
        private readonly Dictionary<Type, TypeInfo[]> m_interfaceImplementersCache = new();
        private readonly Dictionary<Type, TypeInfo[]> m_attributeTypesCache = new();
        private readonly Dictionary<string, Type> m_identifierTypesCache = new();

        private TypeInfo[] m_allTypes;

        public void Initialize()
        {
            m_allTypes = Assembly.GetExecutingAssembly().GetTypes().AsValueEnumerable()
                .Where(type => type.GetCustomAttribute<ReflectionInjectAttribute>() != null)
                .Select(type => type.GetTypeInfo()).ToArray();

            LoadIdentifierTypes();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeInfo[] GetDerivedTypes<T>() where T : class
        {
            return GetDerivedTypes(typeof(T));
        }

        public TypeInfo[] GetDerivedTypes(Type baseType)
        {
            if (m_derivedTypesCache.TryGetValue(baseType, out var cached))
                return cached;

            var result = m_allTypes.AsValueEnumerable()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType))
                .ToArray();

            m_derivedTypesCache[baseType] = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeInfo[] GetInterfaceImplementers<T>() where T : class
        {
            return GetInterfaceImplementers(typeof(T));
        }

        public TypeInfo[] GetInterfaceImplementers(Type interfaceType)
        {
            if (m_interfaceImplementersCache.TryGetValue(interfaceType, out var cached))
                return cached;

            var result = m_allTypes.AsValueEnumerable()
                .Where(t => t.IsClass && !t.IsAbstract && t.ImplementedInterfaces.Contains(interfaceType))
                .ToArray();

            m_interfaceImplementersCache[interfaceType] = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeInfo[] GetTypesWithAttribute<T>() where T : Attribute
        {
            return GetTypesWithAttribute(typeof(T));
        }

        public TypeInfo[] GetTypesWithAttribute(Type attributeType)
        {
            if (m_attributeTypesCache.TryGetValue(attributeType, out var cached))
                return cached;

            var result = m_allTypes.AsValueEnumerable()
                .Where(t => t.GetCustomAttribute(attributeType) != null)
                .ToArray();

            m_attributeTypesCache[attributeType] = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeInfo[] GetAllTypes()
        {
            return m_allTypes;
        }

        private void LoadIdentifierTypes()
        {
            foreach (var typeInfo in m_allTypes)
            {
                var identifierAttribute = typeInfo.GetCustomAttribute<IdentifierAttribute>();
                if (identifierAttribute == null)
                    continue;

                if (m_identifierTypesCache.TryGetValue(identifierAttribute.identifier, out var value))
                {
                    Debug.LogWarning($"Duplicate IdentifierAttribute typeName '{identifierAttribute.identifier}' found. " +
                                     $"Type {typeInfo.FullName} overrides {value.FullName}");
                }

                m_identifierTypesCache[identifierAttribute.identifier] = typeInfo.AsType();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Type GetTypeByIdentifier(string typeName)
        {
            return m_identifierTypesCache.GetValueOrDefault(typeName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetTypeByIdentifier(string typeName, out Type type)
        {
            return m_identifierTypesCache.TryGetValue(typeName, out type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IReadOnlyDictionary<string, Type> GetAllIdentifierTypes()
        {
            return m_identifierTypesCache;
        }
    }
}