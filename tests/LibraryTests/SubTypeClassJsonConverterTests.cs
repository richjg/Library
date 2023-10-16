using Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace LibraryTests
{
    public class SubTypeClassJsonConverterTests
    {
        [Test]
        public void JsonSerialize_SerilalizesTheDerviedTypeProperties()
        {
            var obj = new TestData
            {
                Items =
                {
                    new Concrete1{ Id = "1", Prop1 = "prop1" },
                    new Concrete2{ Id = "2", Prop1 = true },
                    new Concrete3{ Id = "3", Prop1 = -1, Items = { "a", "b", "c" } },
                }
            };

            //act
            var result = JsonConvert.SerializeObject(obj);

            //asset
            Assert.That(result, Is.EqualTo(@"{""Items"":[{""Type"":""Concrete1"",""Prop1"":""prop1"",""Id"":""1""},{""Type"":""Concrete2"",""Prop1"":true,""Id"":""2""},{""Type"":""Concrete3"",""Prop1"":-1,""Items"":[""a"",""b"",""c""],""Id"":""3""}]}"));
        }

        [Test]
        public void JsonSerialize_SerilalizesTheDerviedTypeProperties_WhenSubInheritance()
        {
            var obj = new TestData
            {
                Items =
                {
                    new Concrete4{ Id = "4", Prop1 = -1, Items = { "a", "b", "c" }, Special1 = "Special1"}
                }
            };

            //act
            var result = JsonConvert.SerializeObject(obj);

            //asset
            Assert.That(result, Is.EqualTo(@"{""Items"":[{""Type"":""Concrete4"",""Special1"":""Special1"",""Prop1"":-1,""Items"":[""a"",""b"",""c""],""Id"":""4""}]}"));
        }

        [Test]
        public void JsonDeserialize_DeserilalizesTheDerviedTypes_WhenPropertiesInCSharpCasing()
        {
            string json = @"{""Items"":[{""Type"":""Concrete1"",""Prop1"":""prop1"",""Id"":""1""},{""Type"":""Concrete2"",""Prop1"":true,""Id"":""2""},{""Type"":""Concrete3"",""Prop1"":-1,""Items"":[""a"",""b"",""c""],""Id"":""3""}]}";

            //act
            var result = JsonConvert.DeserializeObject<TestData>(json);

            //asset
            Assert.That(result!.Items.Count, Is.EqualTo(3));
            var item1 = result!.Items[0] as Concrete1;
            Assert.That(item1!.Id, Is.EqualTo("1"));
            Assert.That(item1.Prop1, Is.EqualTo("prop1"));
            var item2 = result.Items[1] as Concrete2;
            Assert.That(item2!.Id, Is.EqualTo("2"));
            Assert.That(item2!.Prop1, Is.EqualTo(true));
            var item3 = result.Items[2] as Concrete3;
            Assert.That(item3!.Id, Is.EqualTo("3"));
            Assert.That(item3.Prop1, Is.EqualTo(-1));
            Assert.That(item3.Items, Is.EqualTo(new[] { "a", "b", "c", }));
        }

        [Test]
        public void JsonDeserialize_DeserilalizesTheDerviedTypes_WhenPropertiesInJsonCasing()
        {
            string json = @"{""items"":[{""type"":""Concrete1"",""Prop1"":""prop1"",""id"":""1""},{""type"":""Concrete2"",""prop1"":true,""id"":""2""},{""type"":""Concrete3"",""prop1"":-1,""items"":[""a"",""b"",""c""],""Id"":""3""}]}";

            //act
            var result = JsonConvert.DeserializeObject<TestData>(json);

            //asset
            Assert.That(result!.Items.Count, Is.EqualTo(3));
            var item1 = result.Items[0] as Concrete1;
            Assert.That(item1!.Id, Is.EqualTo("1"));
            Assert.That(item1.Prop1, Is.EqualTo("prop1"));
            var item2 = result.Items[1] as Concrete2;
            Assert.That(item2!.Id, Is.EqualTo("2"));
            Assert.That(item2.Prop1, Is.EqualTo(true));
            var item3 = result.Items[2] as Concrete3;
            Assert.That(item3!.Id, Is.EqualTo("3"));
            Assert.That(item3.Prop1, Is.EqualTo(-1));
            Assert.That(item3.Items, Is.EqualTo(new[] { "a", "b", "c", }));
        }

        [TestCase("concrete1")]
        [TestCase("CONCRETE1")]
        public void JsonDeserialize_Deserilalizes_WhenTypeKnownCaseInsensitive(string type)
        {
            string json = $@"{{""Items"":[{{""Type"":""{type}"",""Prop1"":""prop1"",""Id"":""1""}}]}}";

            //act
            var result = JsonConvert.DeserializeObject<TestData>(json);

            Assert.That(result!.Items.Count, Is.EqualTo(1));
            Assert.That(result!.Items[0], Is.TypeOf<Concrete1>());
        }


        [Test]
        public void JsonDeserialize_Throws_WhenTypeIsNotKnown()
        {
            string json = @"{""Items"":[{""Type"":""unknown"",""Prop1"":""prop1"",""Id"":""1""}]}";

            //act
            void Act()
            {
                var result = JsonConvert.DeserializeObject<TestData>(json);
            }

            //asset
            Assert.That(Act, Throws.Exception.TypeOf<JsonException>().With.Message.Contains("[{\"type\":\"Concrete1\",\"prop1\":\"\",\"id\":\"\"},{\"type\":\"Concrete2\",\"prop1\":false,\"id\":\"\"},{\"type\":\"Concrete3\",\"prop1\":0,\"items\":[],\"id\":\"\"},{\"type\":\"Concrete4\",\"special1\":\"\",\"prop1\":0,\"items\":[],\"id\":\"\"}]"));
        }

        [Test]
        public void JsonDeserialize_Throws_WhenTypePropertyIsMissing()
        {
            string json = @"{""Items"":[{""Prop1"":""prop1"",""Id"":""1""}]}";

            //act
            void Act()
            {
                var result = JsonConvert.DeserializeObject<TestData>(json);
            }

            //asset
            Assert.That(Act, Throws.Exception.TypeOf<JsonException>().With.Message.Contains(@"object requires a type property, for example. [{""type"":""Concrete1"",""prop1"":"""",""id"":""""},{""type"":""Concrete2"",""prop1"":false,""id"":""""},{""type"":""Concrete3"",""prop1"":0,""items"":[],""id"":""""},{""type"":""Concrete4"",""special1"":"""",""prop1"":0,""items"":[],""id"":""""}]"));
        }

        [Test]
        public void JsonDeserialize_Throws_WhenTypePropertyIsEmpty()
        {
            string json = @"{""Items"":[{""Type"":"""", ""Prop1"":""prop1"",""Id"":""1""}]}";

            //act
            void Act()
            {
                var result = JsonConvert.DeserializeObject<TestData>(json);
            }

            //asset
            Assert.That(Act, Throws.Exception.TypeOf<JsonException>().With.Message.Contains(@"object requires a type property, for example. [{""type"":""Concrete1"",""prop1"":"""",""id"":""""},{""type"":""Concrete2"",""prop1"":false,""id"":""""},{""type"":""Concrete3"",""prop1"":0,""items"":[],""id"":""""},{""type"":""Concrete4"",""special1"":"""",""prop1"":0,""items"":[],""id"":""""}]"));
        }

        [Test]
        public void JsonDeserialize_Throws_WhenTypePropertyIsNull()
        {
            string json = @"{""Items"":[{""Type"":null, ""Prop1"":""prop1"",""Id"":""1""}]}";

            //act
            void Act()
            {
                var result = JsonConvert.DeserializeObject<TestData>(json);
            }

            //asset
            Assert.That(Act, Throws.Exception.TypeOf<JsonException>().With.Message.Contains(@"object requires a type property, for example. [{""type"":""Concrete1"",""prop1"":"""",""id"":""""},{""type"":""Concrete2"",""prop1"":false,""id"":""""},{""type"":""Concrete3"",""prop1"":0,""items"":[],""id"":""""},{""type"":""Concrete4"",""special1"":"""",""prop1"":0,""items"":[],""id"":""""}]"));
        }

        [Test]
        public void JsonDeserialize_DeserializeswhenTypeHasProperty_WhenTypePropertyValueIsSetToABaseType()
        {
            string json = @"{""Property1"":{""Type"":""Concrete1"", ""Prop1"":""value1""}}";

            //act
            var result = JsonConvert.DeserializeObject<TestData2>(json);

            //asset
            Assert.That((result!.Property1 as Concrete1)!.Prop1, Is.EqualTo("value1"));
        }

        [Test]
        public void JsonDeserialize_AllowsAssoicatedTypeObjectTobeNull_WhenTypePropertyValueIsNull()
        {
            string json = @"{""Property1"":null}";

            //act
            var result = JsonConvert.DeserializeObject<TestData2>(json);

            //asset
            Assert.That(result!.Property1, Is.Null);
        }

        [Test]
        public void JsonDeserialize_Throw_WhenJsonIsAJsonString()
        {
            string json = "\"Hello\"";

            //act
            void Act()
            {
                var result = JToken.Parse(json).ToObject<TestBase>();
            }

            //asset
            Assert.That(Act, Throws.Exception.TypeOf<JsonException>().With.Message.Contains(@"object requires a type property, for example. [{""type"":""Concrete1"",""prop1"":"""",""id"":""""},{""type"":""Concrete2"",""prop1"":false,""id"":""""},{""type"":""Concrete3"",""prop1"":0,""items"":[],""id"":""""},{""type"":""Concrete4"",""special1"":"""",""prop1"":0,""items"":[],""id"":""""}]"));
        }

        [Test]
        public void JsonDeserialize_TheBaseClassCanSpecifyThePropertyThatContainsTheTypeNameToUse()
        {
            var json = "{\"Area\":\"Area1\"}";

            //act
            var result = json.FromJson<TestBaseUsingDifferentNameForType>();

            //asset
            Assert.That(result, Is.TypeOf<TestUsingDifferentNameForType1>());
        }

        [Test]
        public void JsonDeserialize_TheBaseClassCanSpecifyTheDefaultTypeValueWhenTheJsonDontHaveOne()
        {
            var json = "[{\"Data\":\"Data\"},{\"Type\":\"Type2\",\"Value\":\"Value\"}]";

            //act
            var result = json.FromJson<List<TestBaseUsingDefaultTypeValue>>();

            //asset
            Assert.That(result![0].Type, Is.EqualTo("Type1"));
            Assert.That(result![1].Type, Is.EqualTo("Type2"));
        }

        public class TestData
        {
            public List<TestBase> Items { get; set; } = new List<TestBase>();
        }

        public class TestData2
        {
            public TestBase? Property1 { get; set; }
        }

        [JsonConverter(typeof(JsonSubTypeConverter<TestBaseUsingDifferentNameForType>), "Area")]
        public abstract class TestBaseUsingDifferentNameForType
        {
            public abstract string Area { get; }
        }

        public class TestUsingDifferentNameForType1 : TestBaseUsingDifferentNameForType
        {
            public override string Area => "Area1";
        }

        public class TestUsingDifferentNameForType2 : TestBaseUsingDifferentNameForType
        {
            public override string Area => "Area2";
        }

        [JsonConverter(typeof(JsonSubTypeConverter<TestBaseUsingDefaultTypeValue>), "Type", "Type1")]
        public abstract class TestBaseUsingDefaultTypeValue
        {
            public abstract string Type { get; }
        }

        public class TestBaseUsingDefaultTypeValue1 : TestBaseUsingDefaultTypeValue
        {
            public override string Type => "Type1";
            public string Data { get; set; } = string.Empty;
        }

        public class TestBaseUsingDefaultTypeValue2 : TestBaseUsingDefaultTypeValue
        {
            public override string Type => "Type2";
            public string Text { get; set; } = string.Empty;
        }


        [JsonConverter(typeof(JsonSubTypeConverter<TestBase>))]
        public abstract class TestBase : ISubTypeClassJsonTypeDescriptor
        {
            public string Id { get; set; } = string.Empty;
            public abstract string Type { get; }
        }

        public class Concrete1 : TestBase
        {
            public override string Type => "Concrete1";
            public string Prop1 { get; set; } = string.Empty;
        }

        public class Concrete2 : TestBase
        {
            public override string Type => "Concrete2";
            public bool Prop1 { get; set; }    
        }

        public class Concrete3 : TestBase
        {
            public override string Type => "Concrete3";
            public int Prop1 { get; set; }
            public List<string> Items { get; set; } = new List<string>();
        }

        public class Concrete4 : Concrete3
        {
            public override string Type => "Concrete4";
            public string Special1 { get; set; } = string.Empty;
        }
    }
}
