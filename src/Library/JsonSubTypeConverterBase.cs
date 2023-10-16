using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Library
{
    public abstract class JsonSubTypeConverterBase<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Null)
            {
                return existingValue;
            }

            var actualType = GetType(token);
            if (actualType == null)
            {
                return new JsonSerializationException("Could not determine underlying type.");
            }

            if (existingValue == null || existingValue.GetType() != actualType)
            {
                var contract = serializer.ContractResolver.ResolveContract(actualType);
                existingValue = contract.DefaultCreator?.Invoke();
            }
            using (var subReader = token.CreateReader())
            {
                serializer.Populate(subReader, existingValue!);
            }
            return existingValue;
        }

        protected abstract Type GetType(JToken jToken);
    }
}
