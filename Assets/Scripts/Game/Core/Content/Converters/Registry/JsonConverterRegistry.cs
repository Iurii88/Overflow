using System;
using Game.Core.Content.Converters.Attributes;
using Game.Core.Content.Converters.Interfaces;
using Game.Core.Reflection;
using Newtonsoft.Json;
using VContainer;

namespace Game.Core.Content.Converters.Registry
{
    public class JsonConverterRegistry
    {
        [Inject] private readonly IReflectionManager m_reflectionManager;
        [Inject] private readonly IObjectResolver m_resolver;

        private JsonSerializerSettings m_settings;

        [Inject]
        public void Initialize()
        {
            m_settings = CreateSettings();
        }

        public JsonSerializerSettings GetSettings() => m_settings;

        private JsonSerializerSettings CreateSettings()
        {
            var converterTypes = m_reflectionManager.GetByAttribute<ContentConverterAttribute>();
            var settings = new JsonSerializerSettings();

            foreach (var converterType in converterTypes)
            {
                var converter = CreateConverter(converterType);
                settings.Converters.Add(converter);
            }

            return settings;
        }

        private JsonConverter CreateConverter(Type converterType)
        {
            var converter = (JsonConverter)Activator.CreateInstance(converterType);
            if (converter is IInjectableConverter injectable)
                injectable.InjectDependencies(m_resolver);
            return converter;
        }
    }
}