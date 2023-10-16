using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Library
{
    public interface ISubTypeClassJsonTypeDescriptor
    {
        string Type { get; }
    }

    public sealed class JsonSubTypeConverter<T> : JsonSubTypeConverterBase<T>
    {
        private readonly TypeLookupProvider typeLookupProvider = new TypeLookupProvider();
        private string propertyName = "Type";
        private string defaultTypeName = "";

        public JsonSubTypeConverter()
        {
        }

        public JsonSubTypeConverter(string propertyName) : this(propertyName, string.Empty)
        {
        }

        public JsonSubTypeConverter(string propertyName, string defaultTypeName)
        {
            if (propertyName.IsTrimmedNullOrEmpty() == false)
            {
                this.propertyName = propertyName;
            }
            this.defaultTypeName = defaultTypeName;
        }

        protected override Type GetType(JToken jToken)
        {
            if (jToken.Type != JTokenType.Object)
            {
                throw GenerateException();
            }

            var jObject = jToken as JObject;

            var typeName = GetTypeNameFromProperty(jObject, propertyName);
            return typeName!.IsTrimmedNullOrEmpty() ? throw GenerateException() : typeLookupProvider.FindType(propertyName, typeName!) ?? throw GenerateException();
        }

        private string? GetTypeNameFromProperty(JObject? jToken, string propertyName)
        {
            if (jToken == null)
            {
                return string.Empty;
            }

            var property = jToken.Properties().FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
            {
                if (defaultTypeName.IsTrimmedNullOrEmpty())
                {
                    return null;
                }

                return defaultTypeName;
            }

            return (string?)property.Value;
        }

        private Exception GenerateException()
        {
            //Note: the JsonException type is picked up by a aspnet newtonsoft filter, so ends up as a view model error not a 500
            var exampleObjects = typeLookupProvider.GetAllTypes(propertyName).ToList();
            return new JsonException($"object requires a {propertyName.ToLowerInvariant()} property, for example. {exampleObjects.ToJsonWithNoTypeNameHandling()}");
        }

        private class TypeLookupProvider
        {
            private static Dictionary<string, Type> lookup = new();
            private Dictionary<string, Type> GetLookup(string propertyName)
            {
                if (lookup.Count == 0)
                {
                    var baseType = typeof(T);
                    var types = AppDomain.CurrentDomain
                                         .GetAssemblies()
                                         .SelectMany(a => a.GetTypesSafe())
                                         .Where(t => t.IsAbstract == false && baseType.IsAssignableFrom(t));

                    lookup = types.Select(t =>
                    {
                        var subType = Activator.CreateInstance(t);
                        var prop = subType!.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
                        if (prop == null)
                            throw new NotImplementedException($"JsonSubTypeConverter type {subType.GetType().Name} missing property {propertyName} that declares what type this is");

                        var typeName = prop.GetValue(subType) as string ?? string.Empty;

                        return (Type: t, TypeName: typeName);
                    }).ToDictionary(o => o.TypeName, o => o.Type, StringComparer.OrdinalIgnoreCase);
                }

                return lookup;
            }
            public Type? FindType(string propertyName, string typeName) => GetLookup(propertyName).TryGetValue(typeName, out var type) ? type : null;
            public IEnumerable<T?> GetAllTypes(string propertyName) => GetLookup(propertyName).Values.Select(t => (T?)Activator.CreateInstance(t));
        }
    }
}
