using Newtonsoft.Json.Linq;
using System.Text;
using System.Web;

namespace LibraryTests
{

    [TestFixture]
    class SystemExtensionsTests
    {
        #region FromJson

        private enum JsonTest
        {
            Value1 = 99
        }

        [Test]
        public void FromJson_UsesDateTimeOffSet_WhenDeserializingToObject()
        {
            var json = @"{""lastPublishedDateTime"":""2020-09-17T10:37:57.9903042+00:00""}";

            //act
            var resultObject = json.FromJson<object>();
            var resultJson = resultObject!.ToJson();

            //assert
            Assert.That((resultObject as JObject)!["lastPublishedDateTime"]!.Value<DateTimeOffset>().ToString("o"), Is.EqualTo("2020-09-17T10:37:57.9903042+00:00"));
            Assert.That(resultJson, Contains.Substring(@"""lastPublishedDateTime"":""2020-09-17T10:37:57.9903042+00:00"""));
        }

        [Test]
        public void FromJsonStream_UsesDateTimeOffSet_WhenDeserializingToObject()
        {
            var json = @"{""lastPublishedDateTime"":""2020-09-17T10:37:57.9903042+00:00""}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            //act
            var resultObject = stream.FromJson<object>();
            var resultJson = resultObject!.ToJson();

            //assert
            Assert.That((resultObject as JObject)!["lastPublishedDateTime"]!.Value<DateTimeOffset>().ToString("o"), Is.EqualTo("2020-09-17T10:37:57.9903042+00:00"));
            Assert.That(resultJson, Contains.Substring(@"""lastPublishedDateTime"":""2020-09-17T10:37:57.9903042+00:00"""));
        }

        #endregion

        #region ToJson

        [Test]
        public void ToJsonFromJsonOfDictionary_KeepsTheKeysTheSame_WhenSerializingCosThinkThisMakesTheMostSense()
        {
            var dic1 = new Dictionary<string, object> { ["lowercasekey"] = 1, ["UPPERCASESKEY"] = 2, ["MiXeDcAsE"] = 3 };
            var result = dic1.ToJson().FromJson<Dictionary<string, object>>();
            //Assert
            Assert.That(result!.ContainsKey("lowercasekey"), Is.EqualTo(true));
            Assert.That(result!.ContainsKey("UPPERCASESKEY"), Is.EqualTo(true));
            Assert.That(result!.ContainsKey("MiXeDcAsE"), Is.EqualTo(true));
        }

        [Test]
        public void ToJson_ReturnsEnumsAsStrings()
        {
            var obj = new { JsonTest = JsonTest.Value1 };

            //act
            var result = obj.ToJson();
            //Assert
            Assert.That(result, Is.EqualTo(@"{""jsonTest"":""Value1""}"));
        }

        [Test]
        public void ToJson_ReturnsObjectPropertiesCamelCased()
        {
            var obj = new { Prop1 = 1, PROP2 = 2, prop3 = new { SubProp1 = 4 } };

            //act
            var result = obj.ToJson();
            //Assert
            Assert.That(result, Is.EqualTo(@"{""prop1"":1,""proP2"":2,""prop3"":{""subProp1"":4}}"));
        }

        #endregion

        #region ConvertToDateTimeOffset

        [Test]
        public void ConvertToDateTimeOffset_ReturnsLocalDate_WhenOffSetSupplied()
        {
            //arrange
            var myDateString = "2019-09-26T14:15:16.111111Z";
            var myDate = DateTime.Parse(myDateString).ToUniversalTime();

            var offset = new TimeSpan(2, 0, 0);

            //act
            var localisedDateTime = myDate.ConvertToDateTimeOffset(offset);

            //assert
            Assert.That(localisedDateTime, Is.EqualTo(DateTimeOffset.Parse("2019-09-26T16:15:16.111111+02:00")));
        }

        [Test]
        public void ConvertToDateTimeOffset_ReturnsSameDate_WhenOffSetIsNull()
        {
            //arrange
            var myDateString = "2019-09-26T14:15:16.111111Z";
            var myDate = DateTime.Parse(myDateString).ToUniversalTime();

            //act
            var localisedDateTime = myDate.ConvertToDateTimeOffset();

            //assert
            Assert.That(localisedDateTime, Is.EqualTo(DateTimeOffset.Parse("2019-09-26T14:15:16.111111+00:00")));
        }


        [Test]
        public void ConvertToDateTimeOffset_ReturnsLocalDate_WhenDateTimeOffsetSupplied()
        {
            //arrange
            var myDateString = "2019-09-26T14:15:16.111111Z";
            var myDate = DateTime.Parse(myDateString).ToUniversalTime();

            var myOffsetDate = DateTimeOffset.Parse("2019-09-26T10:11:12.111111+02:00");

            //act
            var localisedDateTime = myDate.ConvertToDateTimeOffset(myOffsetDate);

            //assert
            Assert.That(localisedDateTime, Is.EqualTo(DateTimeOffset.Parse("2019-09-26T16:15:16.111111+02:00")));
        }


        [Test]
        public void ConvertToDateTimeOffset_ReturnsSameDate_WhenDateTimeOffsetIsNull()
        {
            //arrange
            var myDateString = "2019-09-26T14:15:16.111111Z";
            var myDate = DateTime.Parse(myDateString).ToUniversalTime();

            //act
            var localisedDateTime = myDate.ConvertToDateTimeOffset((DateTimeOffset?)null);

            //assert
            Assert.That(localisedDateTime, Is.EqualTo(DateTimeOffset.Parse("2019-09-26T14:15:16.111111+00:00")));
        }

        #endregion

        #region FromJsonToDictionaryBestEndeavour

        [TestCase(null, "{}")]
        [TestCase("", "{}")]
        [TestCase("[]", "{}")]
        [TestCase(@"{""key1"":""value1"", ""key2"":""value2""}", @"{""key1"":""value1"",""key2"":""value2""}")]
        [TestCase(@"{""key3"":""value1"", ""key4"":""value2""", @"{""key3"":""value1"",""key4"":""value2""}")]
        [TestCase(@"{""key5"":""value1"", ""key6"":""value2", @"{""key5"":""value1"",""key6"":null}")]
        [TestCase(@"{""key7"":""value1"", ""key8""", @"{""key7"":""value1""}")]
        public void TruncatedJson(string json, string expectedJson)
        {
            var result = json.FromJsonToDictionaryBestEndeavour();
            var jsonResult = result.ToJson();
            Assert.That(jsonResult, Is.EqualTo(expectedJson));
        }

        #endregion

        #region ToJsonWithNoTypeNameHandlingTruncated

        [Test]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsJson_WhenInputObjectAndNoTruncationRequired()
        {
            //arrange
            var obj = new { prop1 = "value1" };
            //act
            var result = obj.ToJsonWithNoTypeNameHandlingTruncated(100);
            //assert
            Assert.That(result, Is.EqualTo(obj.ToJsonWithNoTypeNameHandling()));
        }

        [Test]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsJson_WhenInputArrayAndNoTruncationRequired()
        {
            //arrange
            var array = new[] { "value1" };
            //act
            var result = array.ToJsonWithNoTypeNameHandlingTruncated(100);
            //assert
            Assert.That(result, Is.EqualTo(array.ToJsonWithNoTypeNameHandling()));
        }

        [Test]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsJsonTruncated_WhenInputObjectAndRequiresTruncation()
        {
            //arrange
            var obj = new { prop1 = "value1value1value1" };
            //act
            var result = obj.ToJsonWithNoTypeNameHandlingTruncated(maxSize: 25);
            //assert
            Assert.That(result, Is.EqualTo("{\"prop1\":\"value1va…\"}"));
        }

        [Test]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsJsonTruncated_WhenInputArrayAndRequiresTruncation()
        {
            //arrange
            var obj = new[] { "value1value1value1" };
            //act
            var result = obj.ToJsonWithNoTypeNameHandlingTruncated(maxSize: 17);
            //assert
            Assert.That(result, Is.EqualTo("[\"value1va…\"]"));
        }

        [Test]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsJsonTruncated_WhenInputObjectIsComplex()
        {
            //arrange
            var obj = new
            {
                prop1 = "value1value1value1",
                prop2 = 123,
                prop3 = new Dictionary<string, string> { ["key1"] = "key1key1key1key1key1key1", ["key2"] = "key2key2key2key2key2key2" },
                prop4 = new object[]
                {
                    "arraystringarraystring",
                    1234,
                    new { prop1 = "value1value1value1" },
                    new { prop2 = 123 },
                    new { prop3 = new Dictionary<string, string> { ["key1"] = "key1key1key1key1key1key1", ["key2"] = "key2key2key2key2key2key2" } },
                }
            };

            //act
            var result = obj.ToJsonWithNoTypeNameHandlingTruncated(maxSize: 150);
            //assert
            Assert.That(result, Is.EqualTo("{\"prop1\":\"v…\",\"prop2\":123,\"prop3\":{\"key1\":\"ke…\",\"key2\":\"ke…\"},\"prop4\":[\"a…\",1234,{\"prop1\":\"v…\"},{\"prop2\":123},{\"prop3\":{\"key1\":\"ke…\",\"key2\":\"ke…\"}}]}"));
        }

        [TestCase("", "{\"prop1\":\"\"}")]
        [TestCase("1", "{\"prop1\":\"1\"}")]
        [TestCase("11", "{\"prop1\":\"11\"}")]
        [TestCase("111", "{\"prop1\":\"…\"}")]
        [TestCase("1111", "{\"prop1\":\"1…\"}")]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsJson_WhenTheObjectPropertyIs(string value, string expected)
        {
            //arrange
            var obj = new
            {
                prop1 = value,
            };

            //act
            var result = obj.ToJsonWithNoTypeNameHandlingTruncated(maxSize: 1);
            //assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsJson_WhenInputObjectHasANullProperty()
        {
            //arrange
            var obj = new { prop1 = (string?)null };
            //act
            var result = obj.ToJsonWithNoTypeNameHandlingTruncated(1);
            //assert
            Assert.That(result, Is.EqualTo(obj.ToJsonWithNoTypeNameHandling()));
        }

        [Test]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsJson_WhenInputArrayHasANull()
        {
            //arrange
            var obj = new[] { (string?)null };
            //act
            var result = obj.ToJsonWithNoTypeNameHandlingTruncated(1);
            //assert
            Assert.That(result, Is.EqualTo(obj.ToJsonWithNoTypeNameHandling()));
        }

        [Test]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsJsonInCamelcase()
        {
            //arrange
            var obj = new { Prop1 = "value1" };
            //act
            var result = obj.ToJsonWithNoTypeNameHandlingTruncated(100);
            //assert
            Assert.That(result, Is.EqualTo("{\"prop1\":\"value1\"}"));
        }

        [Test]
        public void ToJsonWithNoTypeNameHandlingTruncated_ReturnsDictionaryKeyAsIs()
        {
            //arrange
            var obj = new { Prop1 = new Dictionary<string, string> { ["Key"] = "123" } };
            //act
            var result = obj.ToJsonWithNoTypeNameHandlingTruncated(100);
            //assert
            Assert.That(result, Is.EqualTo("{\"prop1\":{\"Key\":\"123\"}}"));
        }


        #endregion

        #region ToJTokenWithNoTypeNameHandling

        [Test]
        public void ToJTokenWithNoTypeNameHandling_ReturnsNull_WhenNull()
        {
            object? input = null;

            var result = input!.ToJTokenWithNoTypeNameHandling();

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ToJTokenWithNoTypeNameHandling_ReturnsJToken_WhenObjectObject()
        {
            object input = new { Prop1 = "value" };

            var result = input.ToJTokenWithNoTypeNameHandling();

            Assert.That(result!.ToString(Newtonsoft.Json.Formatting.None), Is.EqualTo("{\"prop1\":\"value\"}"));
        }

        [Test]
        public void ToJTokenWithNoTypeNameHandling_ReturnsJToken_WhenObjectArray()
        {
            object input = new object[] { "value ", new { Prop1 = "value" } };

            var result = input.ToJTokenWithNoTypeNameHandling();

            Assert.That(result!.ToString(Newtonsoft.Json.Formatting.None), Is.EqualTo("[\"value \",{\"prop1\":\"value\"}]"));
        }

        [Test]
        public void ToJTokenWithNoTypeNameHandling_ReturnsJToken_ThatSeralizesUsingJsonSerializerSettingsNone()
        {
            object input = new { Prop1 = "value" };

            var result = input.ToJTokenWithNoTypeNameHandling();

            Assert.That(result!.ToString(Newtonsoft.Json.Formatting.None), Is.EqualTo("{\"prop1\":\"value\"}"));
        }


        #endregion

        #region Merge

        [Test]
        public void Merge_ReturnsEmpty_WhenBothEmpty()
        {
            var d1 = new Dictionary<string, object> { };
            var d2 = new Dictionary<string, object> { };

            var result = d1.Merge(d2);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void Merge_ReturnsValues_WhenDic1ValueAndDic2Empty()
        {
            var d1 = new Dictionary<string, object> { ["item1"] = "value1" };
            var d2 = new Dictionary<string, object> { };

            var result = d1.Merge(d2);

            Assert.That(result, Is.EqualTo(new Dictionary<string, object> { ["item1"] = "value1" }));
        }

        [Test]
        public void Merge_ReturnsValues_WhenDic1ValueAndDic2ValueWithDifferentKeys()
        {
            var d1 = new Dictionary<string, object> { ["item1"] = "value1" };
            var d2 = new Dictionary<string, object> { ["item2"] = "value2" };

            var result = d1.Merge(d2);

            Assert.That(result, Is.EqualTo(new Dictionary<string, object> { ["item1"] = "value1", ["item2"] = "value2" }));
        }

        [Test]
        public void Merge_ReturnsValues_WhenDic1ValueAndDic2ValueWithSameKeySameValue()
        {
            var d1 = new Dictionary<string, object> { ["item1"] = "value1" };
            var d2 = new Dictionary<string, object> { ["item1"] = "value1" };

            var result = d1.Merge(d2);

            Assert.That(result, Is.EqualTo(new Dictionary<string, object> { ["item1"] = "value1" }));
        }

        [Test]
        public void Merge_ReturnsValues_WhenDic1ValueAndDic2ValueWithSameKeyDifferentValue()
        {
            var d1 = new Dictionary<string, object> { ["item1"] = "value1" };
            var d2 = new Dictionary<string, object> { ["item1"] = "value2" };

            var result = d1.Merge(d2);

            Assert.That(result, Is.EqualTo(new Dictionary<string, object> { ["item1"] = new[] { "value1", "value2" } }));
        }

        [Test]
        public void Merge_ReturnsValues_WhenTheresDuplicateKeysWithPotenialCollisons()
        {
            var d1 = new Dictionary<string, object> { ["item1"] = "value1", ["item1-1"] = "value2" };
            var d2 = new Dictionary<string, object> { ["item1"] = "value2", ["item1-1"] = "value2" };

            var result = d1.Merge(d2);

            Assert.That(result, Is.EqualTo(new Dictionary<string, object> { ["item1"] = new[] { "value1", "value2" }, ["item1-1"] = "value2" }));
        }

        [Test]
        public void Merge_ReturnsValues_WhenDic1ValueAndDic2ValueWithSameKeyDifferentValueTypes()
        {
            var d1 = new Dictionary<string, object> { ["item1"] = "value1" };
            var d2 = new Dictionary<string, object> { ["item1"] = 2 };

            var result = d1.Merge(d2);

            Assert.That(result, Is.EqualTo(new Dictionary<string, object> { ["item1"] = new object[] { "value1", 2 } }));
        }

        [Test]
        public void Merge_ReturnsValues_WhenDic1ValueAndDic2ValueWithSameKeyDifferentValueObject()
        {
            var d1 = new Dictionary<string, object> { ["item1"] = new { Test = "value1" } };
            var d2 = new Dictionary<string, object> { ["item1"] = new { Test = "value1" } };

            var result = d1.Merge(d2);

            Assert.That(result, Is.EqualTo(new Dictionary<string, object> { ["item1"] = new { Test = "value1" } }));
        }

        #endregion

        #region CountOcurrenencesOf

        [TestCase(null, null, 0)]
        [TestCase(null, "", 0)]
        [TestCase("", null, 0)]
        [TestCase("", "", 0)]
        [TestCase("the quick brown fox jumps over the lazy dog", "", 0)]
        [TestCase("the quick brown fox jumps over the lazy dog", "NotThere", 0)]
        [TestCase("start the quick brown fox jumps over the lazy dog", "start", 1)]
        [TestCase("the quick brown fox jumps over the lazy dog end", "end", 1)]
        [TestCase("the quick brown fox jumps over the lazy dog", "the", 2)]
        [TestCase("this is testing how many spaces there are", " ", 7)]
        [TestCase("the quick brown fox jumps over the lazy dog", "fox jumps", 1)]
        public void CountOcurrenencesOf_ReturnsCount_When(string input, string pattern, int expected)
        {
            var result = input.CountOcurrenencesOf(pattern);
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion

        #region GetStringFromUTF8

        [Test]
        public void GetStringFromUTF8_ReturnsNull_WhenBytesISNull()
        {
            var data = (byte[]?)null;
            //act
            var result = data!.GetStringFromUTF8();
            //assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetStringFromUTF8_ReturnsEmptyString_WhenBytesLengtIs0()
        {
            var data = new byte[0];
            //act
            var result = data.GetStringFromUTF8();
            //assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetStringFromUTF8_ReturnsStringWithoutUtf8Bom_WhenBytesContainsUTF8Bom()
        {
            var data = GetUtf8WithBom("this starts ith the utf8 bom");
            //act
            var result = data.GetStringFromUTF8();

            //assert
            Assert.That(result, Is.EqualTo("this starts ith the utf8 bom"));

            static byte[] GetUtf8WithBom(string text)
            {
                var enc = new UTF8Encoding(true);
                return enc.GetPreamble().Concat(enc.GetBytes(text)).ToArray();
            }
        }

        [Test]
        public void GetStringFromUTF8_ReturnsStringWithoutUtf8Bom_WhenBytesDontContainUTF8Bom()
        {
            var data = GetUtf8WithoutBom("this starts ith the utf8 bom");
            //act
            var result = data.GetStringFromUTF8();

            //assert
            Assert.That(result, Is.EqualTo("this starts ith the utf8 bom"));

            static byte[] GetUtf8WithoutBom(string text)
            {
                var enc = new UTF8Encoding(false);
                return enc.GetPreamble().Concat(enc.GetBytes(text)).ToArray();
            }
        }

        #endregion

        #region HMACSHA256

        [Test]
        public void HMACSHA256_ReturnsBytes_WhenKeyDataIsByteArrayAndKeyIsByteArray()
        {
            var bytesToHmac = new byte[] { 0x44, 0x61, 0x74, 0x61 };
            var key = new byte[] { 0x73, 0x65, 0x63, 0x72, 0x65, 0x74 };

            //act
            var result = bytesToHmac.HMACSHA256(key);

            //assert
            Assert.That(result.Length, Is.EqualTo(32));
            Assert.That(result, Is.EqualTo(new byte[] { 0x5E, 0xD9, 0x3A, 0xAC, 0x30, 0x5C, 0xB5, 0xE4, 0x49, 0xCA, 0x48, 0x72, 0x6A, 0x07, 0x34, 0xF2, 0x76, 0xE2, 0xCD, 0xFE, 0x19, 0xE8, 0x3E, 0x71, 0xB9, 0x6F, 0x2E, 0xBA, 0x67, 0x96, 0x87, 0xBE }));
        }

        [Test]
        public void HMACSHA256_ReturnsBytes_WhenKeyDataIsByteArrayAndKeyIsString()
        {
            var bytesToHmac = new byte[] { 0x44, 0x61, 0x74, 0x61 };
            var key = "secret";

            //act
            var result = bytesToHmac.HMACSHA256(key);

            //Console.WriteLine(BitConverter.ToString(key.GetBytesUTF8()).Replace("-", ",0x"));

            //assert
            Assert.That(result.Length, Is.EqualTo(32));
            Assert.That(result, Is.EqualTo(new byte[] { 0x5E, 0xD9, 0x3A, 0xAC, 0x30, 0x5C, 0xB5, 0xE4, 0x49, 0xCA, 0x48, 0x72, 0x6A, 0x07, 0x34, 0xF2, 0x76, 0xE2, 0xCD, 0xFE, 0x19, 0xE8, 0x3E, 0x71, 0xB9, 0x6F, 0x2E, 0xBA, 0x67, 0x96, 0x87, 0xBE }));
        }

        [Test]
        public void HMACSHA256_ReturnsBytes_WhenKeyDataIsStringAndKeyIsString()
        {
            var toHmac = "Data";
            var key = "secret";

            //act
            var result = toHmac.HMACSHA256(key);

            //assert
            Assert.That(result.Length, Is.EqualTo(32));
            Assert.That(result, Is.EqualTo(new byte[] { 0x5E, 0xD9, 0x3A, 0xAC, 0x30, 0x5C, 0xB5, 0xE4, 0x49, 0xCA, 0x48, 0x72, 0x6A, 0x07, 0x34, 0xF2, 0x76, 0xE2, 0xCD, 0xFE, 0x19, 0xE8, 0x3E, 0x71, 0xB9, 0x6F, 0x2E, 0xBA, 0x67, 0x96, 0x87, 0xBE }));
        }

        [Test]
        public void HMACSHA256_ReturnsBytes_WhenKeyDataAndKeyContainsExtendedCharacters()
        {
            var toHmac = "Data£😂";
            var key = "secret😂";

            //act
            var result = toHmac.HMACSHA256(key);

            //assert
            Assert.That(result.Length, Is.EqualTo(32));
            Assert.That(result, Is.EqualTo(new byte[] { 0xB7, 0xF4, 0xE9, 0x16, 0x01, 0xEB, 0x3D, 0x71, 0xC8, 0xD9, 0x3B, 0x6B, 0x41, 0xF9, 0x40, 0x9A, 0x1B, 0x1A, 0x15, 0x23, 0xDD, 0x61, 0x54, 0xC5, 0x10, 0x04, 0xBF, 0x13, 0xCE, 0xD8, 0x7F, 0xEC }));
        }

        [Test]
        public void HMACSHA256_Throws_WhenDataToHmacIsNull()
        {
            string? toHmac = null;
            string key = "key";

            //act
            void Act()
            {
                var result = toHmac!.HMACSHA256(key);
            }

            //assert
            var exception = Assert.Throws<ArgumentNullException>(Act);
        }

        [Test]
        public void HMACSHA256_Throws_WhenKeyIsNull()
        {
            string toHmac = "Data";
            string? key = null;

            //act
            void Act()
            {
                var result = toHmac.HMACSHA256(key!);
            }

            //assert
            var exception = Assert.Throws<ArgumentNullException>(Act);
        }

        #endregion

        #region ToHexString

        [Test]
        public void ToHexString_ReturnsStringAsHex()
        {
            var bytes = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

            //act
            var result = bytes.ToHexString();

            //assert
            Assert.That(result, Is.EqualTo("000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F202122232425262728292A2B2C2D2E2F303132333435363738393A3B3C3D3E3F404142434445464748494A4B4C4D4E4F505152535455565758595A5B5C5D5E5F606162636465666768696A6B6C6D6E6F707172737475767778797A7B7C7D7E7F808182838485868788898A8B8C8D8E8F909192939495969798999A9B9C9D9E9FA0A1A2A3A4A5A6A7A8A9AAABACADAEAFB0B1B2B3B4B5B6B7B8B9BABBBCBDBEBFC0C1C2C3C4C5C6C7C8C9CACBCCCDCECFD0D1D2D3D4D5D6D7D8D9DADBDCDDDEDFE0E1E2E3E4E5E6E7E8E9EAEBECEDEEEFF0F1F2F3F4F5F6F7F8F9FAFBFCFDFEFF"));
        }

        [Test]
        public void ToHexString_ReturnsEmptyString_WhenByteArrayNull()
        {
            byte[]? bytes = null;

            //act
            var result = bytes!.ToHexString();

            //assert
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void ToHexStringWithOxPrefix_ReturnsStringAsHex()
        {
            var bytes = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

            //act
            var result = bytes.ToHexStringWithOxPrefix();

            //assert
            Assert.That(result, Is.EqualTo("0x000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F202122232425262728292A2B2C2D2E2F303132333435363738393A3B3C3D3E3F404142434445464748494A4B4C4D4E4F505152535455565758595A5B5C5D5E5F606162636465666768696A6B6C6D6E6F707172737475767778797A7B7C7D7E7F808182838485868788898A8B8C8D8E8F909192939495969798999A9B9C9D9E9FA0A1A2A3A4A5A6A7A8A9AAABACADAEAFB0B1B2B3B4B5B6B7B8B9BABBBCBDBEBFC0C1C2C3C4C5C6C7C8C9CACBCCCDCECFD0D1D2D3D4D5D6D7D8D9DADBDCDDDEDFE0E1E2E3E4E5E6E7E8E9EAEBECEDEEEFF0F1F2F3F4F5F6F7F8F9FAFBFCFDFEFF"));
        }

        #endregion

        #region ListKeyValuePair_Add

        [Test]
        public void ListKeyValuePair_AddOrReplace_AddPairToList()
        {
            var list = new List<KeyValuePair<string, string>>();

            //Act
            list.AddOrReplace("key", "value");

            //Assert
            Assert.That(list.ToDictionary(k => k.Key, k => k.Value), Is.EquivalentTo(new Dictionary<string, string> { ["key"] = "value" }));
        }

        [Test]
        public void ListKeyValuePair_AddOrReplace_AddPairToList_ReplacesCaseInsensitive()
        {
            var list = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Key", "original value"), new KeyValuePair<string, string>("key2", "originalvalue2") };

            //Act
            list.AddOrReplace("key", "newvalue");

            //Assert
            Assert.That(list.ToDictionary(k => k.Key, k => k.Value), Is.EquivalentTo(new Dictionary<string, string> { ["key"] = "newvalue", ["key2"] = "originalvalue2" }));
        }

        #endregion

        #region IsSchemeHttpOrHttps

        [Test]
        public void IsSchemeHttpOrHttps_ReturnsFalse_WhenNull()
        {
            Uri? uri = null;
            //act
            var result = uri!.IsSchemeHttpOrHttps();
            //assert
            Assert.That(result, Is.False);
        }

        [TestCase("http://www.test.com")]
        [TestCase("HTTP://www.test.com")]
        [TestCase("https://www.test.com")]
        [TestCase("HTTPS://www.test.com")]
        public void IsSchemeHttpOrHttps_ReturnsTrue_WhenItIs(string uriToTest)
        {
            Uri uri = new Uri(uriToTest);
            //act
            var result = uri.IsSchemeHttpOrHttps();
            //assert
            Assert.That(result, Is.True);
        }

        [TestCase("htthttp://www.test.com")]
        [TestCase("Not://www.test.com")]
        [TestCase("ftp://www.test.com")]
        public void IsSchemeHttpOrHttps_ReturnsFalse_WhenItIs(string uriToTest)
        {
            Uri uri = new Uri(uriToTest);
            //act
            var result = uri.IsSchemeHttpOrHttps();
            //assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region TryPopulateJson

        private class TestObject
        {
            public string Name { get; set; } = string.Empty;
            public int Number { get; set; }
        }

        [TestCase("{\"Name\": \"Bob\",\"Number\": 10}")]    // match to TestJson
        [TestCase("{\"Name\": \"Bob\"}")]
        [TestCase("{}")]
        public void TryPopulateJson_ReturnsTrue_IfTheJsonParsesToTheObject(string json)
        {
            var testObject = new TestObject();
            Assert.That(json.TryPopulateJson(testObject), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\"\"")]    // Note this is valid json, but does not parse to the object
        [TestCase("\"Ok\"")]  // Note this is valid json, but does not parse to the object
        [TestCase("[]")]      // Note this is valid json, but does not parse to the object
        [TestCase("word")]
        public void TryPopulateJson_ReturnsFalse_IfTheObjectDoesNotParseSuccessfully(string json)
        {
            var testObject = new TestObject();
            Assert.That(json.TryPopulateJson(testObject), Is.False);
        }

        [Test]
        public void TryPopulateJson_ReturnsTrue_EvenIfTheAreAdditionalMembersInTheJson()
        {
            var testObject = new TestObject();
            Assert.That("{\"NotAMember\": 10}".TryPopulateJson(testObject), Is.True);
        }

        #endregion

        #region TryParseJson

        public class ApiReceiveEventData
        {
            public string Type { get; set; } = string.Empty;
            public int? IdentityId { get; set; }
            public object? Data { get; set; }    
        }

        [Test]
        public void TryParseJson_ReturnsTrueAndTheObject_IfTheJsonParsesSuccessfully()
        {
            var json = @"{""type"":""DocumentSearchDocumentUploaded"",""data"":{""assistantId"":468,""fileName"":""supple-synapse-302210-8210eb159355.json"",""mimeType"":"""",""indexedContentType"":"""",""size"":2344,""documentCreatedDateTime"":""2021-02-03T11:12:21.5392843+00:00"",""documentModifiedDateTime"":""2021-02-03T11:12:21.5392845+00:00"",""storagePathEncoded"":"""",""calculatedStoragePathEncoded"":"""",""sourceUrl"":null,""keywords"":[],""sourceKey"":""documentsearch"",""description"":null,""status"":""NotIndexed"",""statusLocalised"":""Not indexed"",""errors"":[],""warnings"":[]}}";

            //act
            bool result = json.TryParseJson<ApiReceiveEventData>(out var resultObject);

            //assert
            Assert.That(result, Is.True);
            Assert.That(resultObject, Is.Not.Null);
            Assert.That(resultObject.Type, Is.EqualTo("DocumentSearchDocumentUploaded"));
        }

        [Test]
        public void TryParseJson_WillReturnTrue_EvenIfPassedEmptyJsonObject()
        {
            // arrange
            var json = "{}";

            //act
            bool result = json.TryParseJson<ApiReceiveEventData>(out var resultObject);

            //assert
            Assert.That(result, Is.True);
            Assert.That(resultObject, Is.Not.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\"\"")]    // Note this is valid json, but does not parse to the object
        [TestCase("\"Ok\"")]  // Note this is valid json, but does not parse to the object
        [TestCase("[]")]      // Note this is valid json, but does not parse to the object
        [TestCase("word")]
        public void TryParseJson_ReturnsFalseAndNull_IfTheObjectDoesNotParseSuccessfully(string json)
        {
            //act
            bool result = json.TryParseJson<ApiReceiveEventData>(out var resultObject);

            //assert
            Assert.That(result, Is.False);
            Assert.That(resultObject, Is.Null);
        }

        [Test]
        public void TryParseJson_ReturnsTrue_IfTheAreAdditionalProperties_And_IgnoreMissingMemberIsTrue()
        {
            // Arrange
            var json = "{\"NotAMember\": 10}";

            // Act
            var result = json.TryParseJson<ApiReceiveEventData>(out _);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TryParseJson_UsesDateTimeOffSet_WhenDeserializingToObject()
        {
            var json = @"{""lastPublishedDateTime"":""2020-09-17T10:37:57.9903042+00:00""}";

            //act
            json.TryParseJson<object>(out var resultObject);
            var resultJson = resultObject!.ToJson();

            //assert
            Assert.That((resultObject as JObject)!["lastPublishedDateTime"]!.Value<DateTimeOffset>().ToString("o"), Is.EqualTo("2020-09-17T10:37:57.9903042+00:00"));
            Assert.That(resultJson, Contains.Substring(@"""lastPublishedDateTime"":""2020-09-17T10:37:57.9903042+00:00"""));
        }

        #endregion

        #region IsValidJson

        [TestCase("\"A single string is valid\"")]
        [TestCase("10")]
        [TestCase("{}")]
        [TestCase("{\"prop1\":123}")]
        [TestCase(@"{
""prop1"":123
}")]
        [TestCase("[]")]
        [TestCase("[1,2,3]")]
        [TestCase("[{\"prop1\":1}, {\"prop1\":2}]")]
        [TestCase(@"[
{""prop1"":1}, 
{""prop1"":2}
]")]
        public void IsValidJson_ReturnsTrue_When(string json)
        {
            var result = json.IsValidJson(out _);

            Assert.That(result, Is.True);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("{\"prop1\":123")]
        [TestCase("[")]
        [TestCase("1,2]")]
        [TestCase(@"{
""prop1"":123
")]
        public void IsValidJson_ReturnsFalse_When(string json)
        {
            var result = json.IsValidJson(out _);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsValidJson_ReturnsErrorMessage_WhenInvalid()
        {
            string json = "[,";
            _ = json.IsValidJson(out var errorMessage);

            Assert.That(errorMessage, Contains.Substring("Unexpected end of content while loading JArray"));
        }

        #endregion

        #region JsonDecode

        [TestCase("", "")]
        [TestCase(null, null)]
        [TestCase("noquotes", "noquotes")]
        [TestCase("\\\"quotes\\\"", "\"quotes\"")]
        [TestCase("{\\\"prop1\\\":56}", "{\"prop1\":56}")]
        public void JsonDecode_ReturnsDecodedJson_When(string encodedJson, string expected)
        {
            var result = encodedJson.JsonDecode();

            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion 

        #region ToBase64String

        [Test]
        public void ToBase64String_ReturnsEmpty_WhenNull()
        {
            byte[]? data = null;

            //act
            var result = data!.ToBase64String();

            //assert
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void ToBase64String_ReturnsEmpty_WhenEmpty()
        {
            var data = Array.Empty<byte>();

            //act
            var result = data.ToBase64String();

            //assert
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void ToBase64String_ReturnsBase64String_WhenThereIsData()
        {
            var data = new byte[] { 0 };

            //act
            var result = data.ToBase64String();

            //assert
            Assert.That(result, Is.EqualTo("AA=="));
        }

        #endregion

        #region ToBase64UrlString

        [Test]
        public void ToBase64UrlString_ReturnsEmpty_WhenNull()
        {
            byte[]? data = null;

            //act
            var result = data!.ToBase64UrlString();

            //assert
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void ToBase64UrlString_ReturnsEmpty_WhenEmpty()
        {
            var data = Array.Empty<byte>();

            //act
            var result = data.ToBase64UrlString();

            //assert
            Assert.That(result, Is.EqualTo(""));
        }

        [TestCase("aaaaa?")] //Contains /
        [TestCase("aaaaa>")] //Contains +
        [TestCase("aaaaa")] //Contains = 
        public void ToBase64UrlString_ReturnsBase64StringForUrl_WhenDataIs(string data)
        {
            var result = data.GetBytesUTF8()?.ToBase64UrlString();

            var url = Uri.TryCreate($"http://www.example.com/test/{result}", UriKind.Absolute, out var uri);

            Assert.That(uri!.IsWellFormedOriginalString, Is.True);
            Assert.That(uri!.PathAndQuery.Split("/").Last(), Is.EqualTo(result));
            Assert.That(result!.Contains("/"), Is.False);
            Assert.That(result!.Contains("+"), Is.False);
            Assert.That(result!.Contains("="), Is.False);
        }

        [TestCase("aaaaa?")] //Contains /
        [TestCase("aaaaa>")] //Contains +
        [TestCase("aaaaa")] //Contains = 
        public void ToBase64UrlString_ReturnsBase64StringForUrlQuery_WhenDataIs(string data)
        {
            var result = data.GetBytesUTF8()?.ToBase64UrlString();

            var url = Uri.TryCreate($"http://www.example.com/test/?a={result}", UriKind.Absolute, out var uri);

            Assert.That(uri!.IsWellFormedOriginalString, Is.True);
            Assert.That(HttpUtility.ParseQueryString(uri.Query)["a"], Is.EqualTo(result));
            Assert.That(result!.Contains("/"), Is.False);
            Assert.That(result!.Contains("+"), Is.False);
            Assert.That(result!.Contains("="), Is.False);
        }


        #endregion

        #region IsAllowedFileExtension

        [TestCase("somefile.TXT")]
        [TestCase("SOMEFILE.txt")]
        [TestCase("somefile.pdf")]
        [TestCase("somefile")]
        public void IsAllowedFileExtension_ReturnsTrue_IfFileExtensionIsAllowed(string fileName)
        {
            //arrange
            var allowedFileTypes = new List<string> { ".odt", ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".ods", ".ppt", ".pptx", ".odp", ".rtf", ".txt", ".htm", ".html", ".xml", ".json" };

            //arrangeact
            bool result = fileName.IsAllowedFileExtension(allowedFileTypes);

            //assert
            Assert.That(result, Is.True);
        }

        [TestCase("somefile.EXE")]
        [TestCase("SOMEFILE.exe")]
        [TestCase("somefile.bat")]
        public void IsAllowedFileExtension_ReturnsFalse_IfFileExtensionIsNotAllowed(string fileName)
        {
            //arrange
            var allowedFileTypes = new List<string> { ".odt", ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".ods", ".ppt", ".pptx", ".odp", ".rtf", ".txt", ".htm", ".html", ".xml", ".json" };

            //arrangeact
            bool result = fileName.IsAllowedFileExtension(allowedFileTypes);

            //assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region Sha256

        [Test]
        public void HashSHA1AsHex_Returns_WhenEmpty()
        {
            var result = "".HashSHA1AsHex()!;
            //Assert
            Assert.That(result.Length, Is.EqualTo(40));
            Assert.That(result, Is.EqualTo("DA39A3EE5E6B4B0D3255BFEF95601890AFD80709"));
        }

        [Test]
        public void HashSHA1AsHex_Returns_WhenNull()
        {
            var result = ((string?)null).HashSHA1AsHex()!;
            //Assert
            Assert.That(result.Length, Is.EqualTo(40));
            Assert.That(result, Is.EqualTo("DA39A3EE5E6B4B0D3255BFEF95601890AFD80709"));
        }

        [Test]
        public void HashSHA1AsHex_Returns_ForData()
        {
            var result = "data".HashSHA1AsHex()!;
            //Assert
            Assert.That(result.Length, Is.EqualTo(40));
            Assert.That(result, Is.EqualTo("A17C9AAA61E80A1BF71D0D850AF4E5BAA9800BBD"));
        }

        [Test]
        public void HashSHA1_Returns_WhenNull()
        {
            var result = ((byte[]?)null).HashSHA1();
            //Assert
            Assert.That(result, Is.EqualTo(null));
        }

        #endregion

        #region ToWords

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ToWords_ReturnsEmpty_WhenGivenNullOrEmpty(string input)
        {
            var result = input.ToWords();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ToWords_ReturnsStrings_SplittingOnWhitespace()
        {
            var result = "This is my string".ToWords();

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result, Is.EquivalentTo(new List<string> { "This", "is", "my", "string" }));
        }

        [Test]
        public void ToWords_ReturnsStrings_IgnoresPunctuationInWords()
        {
            var result = "This-is!my.stri!ng".ToWords();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result, Is.EquivalentTo(new List<string> { "This-is!my.stri!ng" }));
        }

        [Test]
        public void ToWords_ReturnsStrings_RemovesPunctuationAroundWords()
        {
            var result = "?My 'name' is davey-jones!".ToWords();

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result, Is.EquivalentTo(new List<string> { "My", "name", "is", "davey-jones" }));
        }

        [Test]
        public void ToWords_ReturnsStrings_RemovesMultiplePunctuation()
        {
            var result = "My name is davey-jones!!?!".ToWords();

            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result, Is.EquivalentTo(new List<string> { "My", "name", "is", "davey-jones" }));
        }

        #endregion
    }
}
