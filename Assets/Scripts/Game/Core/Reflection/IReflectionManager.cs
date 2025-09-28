using System;
using System.Collections.Generic;
using System.Reflection;

namespace Game.Core.Reflection
{
    public interface IReflectionManager
    {
        TypeInfo[] GetDerivedTypes<T>() where T : class;
        TypeInfo[] GetDerivedTypes(Type baseType);
        TypeInfo[] GetInterfaceImplementers<T>() where T : class;
        TypeInfo[] GetInterfaceImplementers(Type interfaceType);
        TypeInfo[] GetTypesWithAttribute<T>() where T : Attribute;
        TypeInfo[] GetTypesWithAttribute(Type attributeType);
        Type GetTypeByIdentifier(string typeName);
        bool TryGetTypeByIdentifier(string typeName, out Type type);
        IReadOnlyDictionary<string, Type> GetAllIdentifierTypes();
        TypeInfo[] GetAllTypes();
    }
}