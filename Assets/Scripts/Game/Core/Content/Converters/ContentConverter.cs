using System;
using System.Collections.Generic;
using Game.Core.Content.Converters.Interfaces;
using Game.Core.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VContainer;

namespace Game.Core.Content.Converters
{
    public class ContentConverter<T> : JsonConverter<T>, IInjectableConverter
    {
        protected virtual string fieldName { get; } = "identifier";

        private IReflectionManager m_reflectionManager;

        public void InjectDependencies(IObjectResolver resolver)
        {
            m_reflectionManager = resolver.Resolve<IReflectionManager>();
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var typeValue = jObject[fieldName]?.Value<string>();
            if (string.IsNullOrEmpty(typeValue))
                throw new JsonSerializationException("Missing 'type' field in ContentProperty");

            if (!m_reflectionManager.TryGetTypeByIdentifier(typeValue, out var targetType))
                throw new KeyNotFoundException($"No type found with Identifier '{typeValue}'");

            var instance = (T)Activator.CreateInstance(targetType);
            serializer.Populate(jObject.CreateReader(), instance);
            return instance;
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}