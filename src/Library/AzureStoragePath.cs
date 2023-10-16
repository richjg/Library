using System.Text;

namespace Library
{
    public class AzureStoragePath
    {
        public static string Encode(string resourceGroupName, string assistantName, string filename)
        {
            // With the choices here the problem characters are:
            // \ and /  - this is because they are interpreted by blob storage as folder seperators
            // #?       - these are not encoded by the Uri class, and when you upload to blob storage it gets confused about what you mean
            // %        - it's not clear whats going on with % but it is a problem somwhere

            // For a nice table of different url encoding schemes see https://secretgeek.net/uri_enconding 
            // Use the Uri class here to encode the url of the file, this is so we 
            // match the encoding applied in AzureStorageService.CreateBlob() (follow the code inside PutBytesAsync())
            var uri = new Uri($"https://{resourceGroupName.ToLower()}.blob.core.windows.net/{assistantName.ToLower()}-documentsearch/{filename}");

            // Once you have the (url encoded) url you have to convert it to a web safe base64 string.
            // This has to match the legacy functionality provided by the full fat framework HttpServerUtility.UrlTokenEncode
            // see https://docs.microsoft.com/en-us/azure/search/search-indexer-field-mappings#base64EncodeFunction
            // and https://stackoverflow.com/questions/50731397/httpserverutility-urltokenencode-replacement-for-netstandard
            // and https://stackoverflow.com/a/26354677
            // Basically replace + with - and / with _ then strip of = and replace with an int equal to the number of equals that were removed.
            var base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(uri.AbsoluteUri));
            var equalsCount = base64Encoded.Count(c => c.Equals('='));
            var legacyUrlSafeBase64 = base64Encoded.TrimEnd('=').Replace('+', '-').Replace('/', '_') + equalsCount.ToString();
            return legacyUrlSafeBase64;
        }

        public static string Decode(string legacyUrlSafeBase64)
        {
            var equalsCount = int.Parse(legacyUrlSafeBase64.Substring(legacyUrlSafeBase64.Length - 1));
            var base64Encoded = legacyUrlSafeBase64.Substring(0, legacyUrlSafeBase64.Length - 1).Replace('-', '+').Replace('_', '/') + new string('=', equalsCount);
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64Encoded));
        }
    }
}