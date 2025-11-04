using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Game.Core.Content.Attributes;
using Game.Core.Logging;
using Game.Core.Reflection.Attributes;
using VContainer;
using ZLinq;

namespace Game.Core.Reflection
{
    public sealed class ReflectionManager : IReflectionManager
    {
        private static readonly HashSet<string> GameAssemblies = new()
        {
            "Core",
            "Game.Editor"
        };

        [Inject]
        private IObjectResolver m_objectResolver;

        private readonly Dictionary<Type, TypeInfo[]> m_derivedTypesCache = new();
        private readonly Dictionary<Type, TypeInfo[]> m_interfaceImplementersCache = new();
        private readonly Dictionary<Type, TypeInfo[]> m_attributeTypesCache = new();
        private readonly Dictionary<string, Type> m_identifierTypesCache = new();

        private TypeInfo[] m_allTypes;

        public void Initialize()
        {
            m_allTypes = GameAssemblies
                .AsValueEnumerable()
                .Select(LoadAssemblySafe)
                .Where(assembly => assembly != null)
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.GetCustomAttribute<ReflectionInjectAttribute>() != null)
                .Select(type => type.GetTypeInfo())
                .ToArray();

            LoadIdentifierTypes();
        }

        private static Assembly LoadAssemblySafe(string assemblyName)
        {
            try
            {
                return Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                GameLogger.Warning($"Failed to load assembly '{assemblyName}': {ex.Message}");
                return null;
            }
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
        public TypeInfo[] GetByInterface<T>() where T : class
        {
            return GetByInterface(typeof(T));
        }

        public TypeInfo[] GetByInterface(Type interfaceType)
        {
            if (m_interfaceImplementersCache.TryGetValue(interfaceType, out var cached))
                return cached;

            var result = m_allTypes.AsValueEnumerable()
                .Where(t => t.IsClass && !t.IsAbstract && t.ImplementedInterfaces.AsValueEnumerable().Contains(interfaceType))
                .ToArray();

            m_interfaceImplementersCache[interfaceType] = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeInfo[] GetByAttribute<T>() where T : Attribute
        {
            return GetByAttribute(typeof(T));
        }

        public TypeInfo[] GetByAttribute(Type attributeType)
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
                    GameLogger.Warning($"Duplicate IdentifierAttribute typeName '{identifierAttribute.identifier}' found. " +
                                       $"Type {typeInfo.FullName} overrides {value.FullName}");

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