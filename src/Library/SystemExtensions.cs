using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

// ReSharper disable CheckNamespace
namespace Library
{
    public static class SystemExtensions
    {
        const int JSONREADER_MAXDEPTH = 1000;

        private static readonly IContractResolver DefaultContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() { ProcessDictionaryKeys = false } };
        private static readonly JsonSerializerSettings JsonSerializerSettingsAuto = new JsonSerializerSettings { MaxDepth = JSONREADER_MAXDEPTH, DateParseHandling = DateParseHandling.DateTimeOffset, TypeNameHandling = TypeNameHandling.Auto, Converters = new List<JsonConverter> { new StringEnumConverter() }, ContractResolver = DefaultContractResolver };
        private static readonly JsonSerializerSettings JsonSerializerSettingsAutoIndented = new JsonSerializerSettings { MaxDepth = JSONREADER_MAXDEPTH, DateParseHandling = DateParseHandling.DateTimeOffset, TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented, Converters = new List<JsonConverter> { new StringEnumConverter() }, ContractResolver = DefaultContractResolver };
        private static readonly JsonSerializerSettings JsonSerializerSettingsNone = new JsonSerializerSettings { MaxDepth = JSONREADER_MAXDEPTH, DateParseHandling = DateParseHandling.DateTimeOffset, TypeNameHandling = TypeNameHandling.None, Converters = new List<JsonConverter> { new StringEnumConverter() }, ContractResolver = DefaultContractResolver };
        private static readonly JsonSerializerSettings JsonSerializerSettingsNoneIndented = new JsonSerializerSettings { MaxDepth = JSONREADER_MAXDEPTH, DateParseHandling = DateParseHandling.DateTimeOffset, TypeNameHandling = TypeNameHandling.None, Formatting = Formatting.Indented, Converters = new List<JsonConverter> { new StringEnumConverter() }, ContractResolver = DefaultContractResolver };

        public static JToken? ToJTokenWithNoTypeNameHandling(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return JToken.FromObject(obj, JsonSerializer.Create(JsonSerializerSettingsNone));
        }

        public static string ToJsonWithNoTypeNameHandling(this object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSerializerSettingsNone);
        }
        public static string ToJsonWithNoTypeNameHandlingIndented(this object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSerializerSettingsNoneIndented);
        }
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSerializerSettingsAuto);
        }
        public static string ToJsonIndented(this object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSerializerSettingsAutoIndented);
        }
        /// <summary>
        /// Will trim string values (property or array) trying to get the total json size to <= maxSize. Not that might actually be possible
        /// </summary>
        public static string ToJsonWithNoTypeNameHandlingTruncated(this object obj, int maxSize)
        {
            if (obj == null)
            {
                return "";
            }

            var jToken = obj.ToJTokenWithNoTypeNameHandling();
            if (jToken == null)
            {
                return "";
            }

            while (jToken.ToJsonWithNoTypeNameHandling().Length > maxSize)
            {
                var path = getPathWithLongestStringValue(jToken);
                if (path.IsTrimmedNullOrEmpty())
                {
                    //no string nowt we can do
                    break;
                }

                var longestStringToken = jToken.SelectToken(path) as JValue;
                if (longestStringToken == null)
                {
                    //is this possible?
                    break;
                }

                var strValue = (string?)longestStringToken.Value;

                if (strValue == null)
                {
                    //is this possible?
                    break;
                }

                if (strValue.Length <= 2)
                {
                    break;
                }

                longestStringToken.Value = strValue.Substring(0, strValue.Length / 2 - 1) + '…';
            }

            return jToken.ToJsonWithNoTypeNameHandling();

            static string getPathWithLongestStringValue(JToken jToken)
            {
                var biggestValueLength = 0;
                var biggestPath = "";
                using var reader = jToken.CreateReader();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.String)
                    {
                        var readerValueLength = ((string?)reader.Value)?.Length ?? 0;
                        if (readerValueLength > biggestValueLength)
                        {
                            biggestValueLength = readerValueLength;
                            biggestPath = reader.Path;
                        }
                    }
                }

                return biggestPath;
            }
        }
        public static T? FromJson<T>(this string str)
        {
            if (str.IsTrimmedNullOrEmpty())
            {
                return default;
            }
            return JsonConvert.DeserializeObject<T>(str, JsonSerializerSettingsAuto);
        }
        public static T? FromJson<T>(this Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return (T?)JsonSerializer.Create(JsonSerializerSettingsAuto).Deserialize(streamReader, typeof(T));
            }
        }

        public static bool TryParseJson<T>(this string str, out T? result) => str.TryParseJson(out result, out _);

        public static bool TryParseJson<T>(this string str, out T? result, out Exception? error)
        {
            /* ************************************************************************************************************ */
            // MissingMemberHandling only refers to properties in the Json that do not exist in T.
            // Json properties are not required by default, meaning an empty JSON object will always parse regardless of T.
            // A Json object with any properties not defined in T will be subject to the MissingMemberHandling setting.
            // See https://github.com/JamesNK/Newtonsoft.Json/issues/1655 and https://www.newtonsoft.com/json/help/html/P_Newtonsoft_Json_JsonSerializerSettings_MissingMemberHandling.htm
            /* ************************************************************************************************************ */
            bool success = true;
            error = null;
            if (str.IsTrimmedNullOrEmpty())
            {
                result = default;
                return false;
            }

            var serializerError = (Exception?)null;
            var settings = new JsonSerializerSettings
            {
                MaxDepth = JSONREADER_MAXDEPTH,
                TypeNameHandling = TypeNameHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Error = (sender, args) =>
                {
                    success = false;
                    args.ErrorContext.Handled = true;
                    serializerError = args.ErrorContext.Error;
                },
            };
            result = JsonConvert.DeserializeObject<T>(str, settings);
            error = serializerError;

            if (result == null)
                return false;

            return success;
        }

        public static bool TryPopulateJson<T>(this string str, T obj)
        {
            /* ************************************************************************************************************ */
            // Note. MissingMemberHandling only refers to properties in the Json that do not exist in T.
            // Json properties are not required by default, meaning an empty JSON object will always parse regardless of T.
            // A Json object with any propertie not defined in T will be subject to the MissingMemberHandling setting.
            // See https://github.com/JamesNK/Newtonsoft.Json/issues/1655 and https://www.newtonsoft.com/json/help/html/P_Newtonsoft_Json_JsonSerializerSettings_MissingMemberHandling.htm
            /* ************************************************************************************************************ */
            if (str.TryParseJson<T>(out _) == false)
            {
                return false;
            }

            bool success = true;
            var settings = new JsonSerializerSettings
            {
                MaxDepth = JSONREADER_MAXDEPTH,
                TypeNameHandling = TypeNameHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateParseHandling = DateParseHandling.DateTimeOffset,
                Error = (sender, args) => { success = false; args.ErrorContext.Handled = true; },
            };
            try
            {
                JsonConvert.PopulateObject(str, obj!, settings);
            }
            catch (JsonSerializationException)
            {
                return false;
            }

            return success;
        }

        /// <summary>
        /// Note single string or a number is valid json
        /// </summary>
        /// <param name="str"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool IsValidJson(this string str, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                _ = JToken.Parse(str);
                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
        }

        /// <summary>
        /// Extract line and position from error message
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static (int line, int position) GetErrorPosition(this string errorMessage)
        {
            var matches = new Regex(", line ([0-9]*), position ([0-9]*).$").Match(errorMessage);
            if (matches != null && matches.Groups.Count == 3)
            {
                return (int.Parse(matches.Groups[1].Value), int.Parse(matches.Groups[2].Value));
            }
            return (0, 0);
        }

        /// <summary>
        /// Note single string or a number is valid json
        /// </summary>
        /// <param name="str"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static bool IsValidJPath(this string jsonPath, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                var json = JToken.Parse("{}");
                json.SelectTokens(jsonPath);
                return true;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return false;
            }
        }

        /// <summary>
        /// returns the value that is has managed to deserialize, 
        /// i.e. handles if the json string is truncated. So you might get some of the values.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Dictionary<string, object> FromJsonToDictionaryBestEndeavour(this string json)
        {
            if (json.IsTrimmedNullOrEmpty())
            {
                return new Dictionary<string, object>();
            }

            try
            {
                if (GetJToken(json) is JToken jToken && jToken.Type == JTokenType.Object)
                {
                    return jToken.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
                }
            }
            catch
            {
            }
            return new Dictionary<string, object>();

            JToken? GetJToken(string jsonString)
            {
                using (var textReader = new StringReader(jsonString))
                using (var jsonReader = new JsonTextReader(textReader))
                using (JTokenWriter jsonWriter = new JTokenWriter())
                {
                    try
                    {
                        jsonWriter.WriteToken(jsonReader);
                    }
                    catch (JsonReaderException)
                    {
                    }

                    return jsonWriter.Token;
                }
            }
        }
        /// <summary>
        /// decodes - encoded json 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string? JsonDecode(this string value)
        {
            if (value.IsTrimmedNullOrEmpty())
            {
                return value;
            }

            return $"\"{value}\"".FromJson<string>();
        }

        public static DateTimeOffset ConvertToDateTimeOffset(this DateTime date, TimeSpan? offset = null)
        {
            var dateTimeOffset = new DateTimeOffset(date.EnsureUtc());
            if (offset != null)
            {
                dateTimeOffset = dateTimeOffset.ToOffset(offset.Value);

            }
            return dateTimeOffset;
        }
        public static DateTimeOffset ConvertToDateTimeOffset(this DateTime date, DateTimeOffset? dateTimeOffsetForCompare) => date.ConvertToDateTimeOffset(dateTimeOffsetForCompare?.Offset);
        public static string ToFormattedShortDateTimeString(this DateTimeOffset date)
        {
            return date.ToString("g");
        }
        public static DateTime? EnsureUtc(this DateTime? datetime)
        {
            if (datetime.HasValue)
                return datetime.Value.EnsureUtc();
            else
                return null;
        }
        public static DateTime EnsureUtc(this DateTime datetime)
        {
            switch (datetime.Kind)
            {
                case DateTimeKind.Local:
                    return datetime.ToUniversalTime();
                case DateTimeKind.Utc:
                    return datetime;
                case DateTimeKind.Unspecified:
                    return new DateTime(datetime.Ticks, DateTimeKind.Utc);
                default:
                    throw new InvalidOperationException("Unhandled DateTimeKind {0}".FormatValue(datetime.Kind));
            }
        }
        public static Task<T> AsCompletedTask<T>(this T t) => Task.FromResult(t);

        /// <summary>
        /// If the byte[] has a BOM then it will use that to convert to string
        /// If the byte[] hasnt got a BOM then it expect the byte[] to be UTF8 and will use UTF8 to decode.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>byte[] as string WITHOUT the BOM</returns>
        public static string? GetStringFromUTF8(this byte[] bytes)
        {
            if (bytes == null)
                return null;

            //below handles BOM where as this dont UTF8Encoding.UTF8.GetString(bytes) and this dont new UTF8Encoding(false).GetString(bytes);
            using var ms = new MemoryStream(bytes);
            using var sr = new StreamReader(ms, Encoding.UTF8); //this handles BOM or no BOM and will remove it from the return string

            return sr.ReadToEnd();
        }

        public static byte[]? GetBytesUTF8(this string value) => value.IsTrimmedNullOrEmpty() ? null : Encoding.UTF8.GetBytes(value);
        public static byte[] HMACSHA256(this byte[] bytesToHash, string key) => bytesToHash.HMACSHA256(Encoding.UTF8.GetBytes(key));
        public static byte[] HMACSHA256(this string stringToHash, string key) => Encoding.UTF8.GetBytes(stringToHash).HMACSHA256(Encoding.UTF8.GetBytes(key));
        public static byte[] HMACSHA256(this byte[] bytesToHash, byte[] keyBytes)
        {
            using var hasher = new HMACSHA256(keyBytes);
            return hasher.ComputeHash(bytesToHash);
        }

        public static string HashSHA1AsHex(this string? data) => data!.HashSHA1()!.ToHexString() ?? string.Empty;
        public static byte[]? HashSHA1(this string? data) => data == null ? Array.Empty<byte>().HashSHA1() : Encoding.UTF8.GetBytes(data).HashSHA1();
        public static byte[]? HashSHA1(this byte[]? data) => data == null ? null : SHA1.HashData(data);


        private static readonly char[] HexLookup = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static string ToHexString(this byte[] data) => data.ToHexString(false);
        public static string ToHexStringWithOxPrefix(this byte[] data) => data.ToHexString(true);
        public static string ToHexString(this byte[] data, bool include_0x_Prefix)
        {
            if (data == null)
            {
                return string.Empty;
            }

            var content = new char[data.Length * 2];
            var output = 0;
            byte d;
            for (var input = 0; input < data.Length; input++)
            {
                d = data[input];
                content[output++] = HexLookup[d / 0x10];
                content[output++] = HexLookup[d % 0x10];
            }

            if (include_0x_Prefix)
            {
                return $"0x{new string(content)}";
            }

            return new string(content);
        }

        public static string ToBase64String(this byte[] data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            return Convert.ToBase64String(data);
        }

        public static string ToBase64UrlString(this byte[] data)
        {
            return data.ToBase64String().Replace("/", "_").Replace("+", "-").Replace("=", "");
        }

        public static string? ConvertFromBase64(this string b64String)
        {
            return Convert.FromBase64String(b64String).GetStringFromUTF8();
        }

        public static bool IsSchemeHttpOrHttps(this Uri uri)
        {
            if (uri == null)
            {
                return false;
            }

            string scheme = uri.Scheme;
            return scheme.Equals("http", StringComparison.OrdinalIgnoreCase) || scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        }

        public static int? ToIntNullable(this string s)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return null;
        }
        public static T CastTo<T>(this object obj)
        {
            return (T)obj;
        }

        public async static Task<byte[]> ToByteArray(this object data)
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(true));
            await streamWriter.WriteAsync(data.ToJsonWithNoTypeNameHandlingIndented());
            streamWriter.Flush();

            return memoryStream.ToArray();
        }
    }
}
