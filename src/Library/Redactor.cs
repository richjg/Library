using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Library
{
    public class Redactor
    {
        //NOTE ***** if you alter this then check UI code as its has its own copy 
        private static readonly List<string> DenyList = new() { "authorization", "pass", "password", "basic", "secret", "ocp-apim-subscription-key", "endpointkey", "api-key", "apikey", "token" };
        private static readonly List<string> AllowList = new() { "TotalTokens", "total_tokens", "CompletionTokens", "completion_tokens", "PromptTokens", "prompt_tokens", "MaxTokens", "max_tokens" };
        public static bool ContainsRedactedName(string name)
        {
            if (AllowList.Any(r => name.Equals(r, StringComparison.OrdinalIgnoreCase)))
                return false;

            return DenyList.Any(r => name.Contains(r, StringComparison.OrdinalIgnoreCase));
        }

        public static readonly Redactor Instance = new();

        public string RedactText => "**Redacted**";


        public string Redact(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            return Redact(headers.ToDictionary(x => x.Key, x => x.Value).ToJsonWithNoTypeNameHandling());
        }
        public string Redact(IEnumerable<KeyValuePair<string, string>> headers)
        {
            return Redact(headers.ToDictionary(x => x.Key, x => x.Value).ToJsonWithNoTypeNameHandling());
        }
        public string Redact(string json) => RedactObjectImp(json, true)?.ToString() ?? string.Empty;
        public object? RedactObject(object obj) => RedactObjectImp(obj);
        private object? RedactObjectImp(object obj, bool indentWhenStringJson = false)
        {
            if (obj == null)
            {
                return obj;
            }

            var objectType = obj.GetType();

            if (objectType.IsPrimitive || objectType.IsEnum || obj is decimal)
            {
                //The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
                return obj;
            }

            if (obj is string str)
            {
                try
                {
                    var s = str.TrimSafe();
                    if (s.StartsWith("{") && s.EndsWith("}") || s.StartsWith("[") && s.EndsWith("]"))
                    {
                        var json = JToken.Parse(str);
                        return (RedactObject(json) as JToken)?.ToString(indentWhenStringJson ? Formatting.Indented : Formatting.None) ?? string.Empty;
                    }
                }
                catch { }

                return obj;
            }

            return JToken.FromObject(obj, JsonSerializer.Create(new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new RedactJsonConverter(RedactText, this)
                }
            }));
        }

        public class RedactJsonConverter : JsonConverter
        {
            private readonly string redactValue;
            private readonly Redactor redactor;

            private static readonly IContractResolver DefaultContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() { ProcessDictionaryKeys = false } };
            private static readonly JsonSerializerSettings JsonSerializerSettingsAuto = new JsonSerializerSettings
            {
                MaxDepth = 1000,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                TypeNameHandling = TypeNameHandling.None,
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                ContractResolver = DefaultContractResolver
            };

            public RedactJsonConverter(string redactValue, Redactor redactor)
            {
                this.redactValue = redactValue;
                this.redactor = redactor;
            }
            private bool redactIfNextPropertyIsValue = false;
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    return;
                }

                JToken jValue = JToken.FromObject(value, JsonSerializer.Create(JsonSerializerSettingsAuto));

                if (jValue.Type == JTokenType.Object)
                {
                    JObject jObjectValue = (JObject)jValue;
                    writer.WriteStartObject();
                    foreach (var p in jObjectValue.Properties())
                    {
                        writer.WritePropertyName(p.Name);
                        if (ContainsRedactedName(p.Name))
                        {
                            if (p.Value.Type == JTokenType.Array)
                            {
                                writer.WriteStartArray();
                                writer.WriteValue(redactValue);
                                writer.WriteEnd();
                            }
                            else
                            {
                                writer.WriteValue(redactValue);
                            }
                        }
                        else
                        {
                            serializer.Serialize(writer, p.Value);
                        }
                    }
                    writer.WriteEnd();
                    return;
                }

                if (jValue.Type == JTokenType.Array)
                {
                    writer.WriteStartArray();
                    JArray jArrayValue = (JArray)jValue;
                    foreach (var jTokenValue in jArrayValue)
                    {
                        serializer.Serialize(writer, jTokenValue);
                    }
                    writer.WriteEnd();
                    return;
                }

                if (writer.WriteState == WriteState.Property || writer.WriteState == WriteState.Array)
                {
                    var ifThisPropertyIsValueThenRedact = redactIfNextPropertyIsValue;
                    redactIfNextPropertyIsValue = false;

                    if (jValue is JValue)
                    {
                        var propertyName = GetPropertyName(writer.Path);
                        string str = jValue.ToString();
                        if (ContainsRedactedName(propertyName))
                        {
                            value = redactValue;
                        }
                        else if (propertyName.Equals("key", StringComparison.OrdinalIgnoreCase) && ContainsRedactedName(str))
                        {
                            //this is assuming what we are serializing a KeyValuePair => key property then value property...
                            redactIfNextPropertyIsValue = true;
                        }
                        else if (ifThisPropertyIsValueThenRedact && propertyName.Equals("value", StringComparison.OrdinalIgnoreCase))
                        {
                            value = redactValue;
                        }
                        else if (jValue.Type == JTokenType.String)
                        {
                            value = redactor.RedactObject(str);
                        }
                        else if (jValue.Type == JTokenType.Date)
                        {
                            value = (DateTimeOffset)jValue;
                        }
                    }
                }

                writer.WriteValue(value);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            public override bool CanRead => false;
            public override bool CanConvert(Type objectType) => true;
            private string GetPropertyName(string path)
            {
                var lastPath = path.Split('.').Last();
                if (lastPath.EndsWith("']"))
                {
                    lastPath = lastPath.Substring(0, lastPath.Length - 2);
                }
                return lastPath;
            }
        }
    }
}
