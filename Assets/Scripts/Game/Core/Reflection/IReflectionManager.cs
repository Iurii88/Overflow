using System;
using System.Collections.Generic;
using System.Reflection;
using VContainer.Unity;

namespace Game.Core.Reflection
{
    public interface IReflectionManager : IInitializable
    {
        TypeInfo[] GetDerivedTypes<T>() where T : class;
        TypeInfo[] GetDerivedTypes(Type baseType);
        TypeInfo[] GetByInterface<T>() where T : class;
        TypeInfo[] GetByInterface(Type interfaceType);
        TypeInfo[] GetByAttribute<T>() where T : Attribute;
        TypeInfo[] GetByAttribute(Type attributeType);
        Type GetTypeByIdentifier(string typeName);
        bool TryGetTypeByIdentifier(string typeName, out Type type);
        IReadOnlyDictionary<string, Type> GetAllIdentifierTypes();
        TypeInfo[] GetAllTypes();
    }
}