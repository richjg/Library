using System.Net.Http.Headers;
using Library;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LibraryTests
{
    public class ReactorTests
    {
        private JsonSerializerSettings JsonSerializerSettingsNone = new JsonSerializerSettings { MaxDepth = 1000, DateParseHandling = DateParseHandling.DateTimeOffset, TypeNameHandling = TypeNameHandling.None, Converters = new List<JsonConverter> { new StringEnumConverter() }, ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() { ProcessDictionaryKeys = false } } };

        #region RedactJson

        [TestCase("")]
        [TestCase(null)]
        public void RedactJson_ReturnsInput_WhenNotSet(string input)
        {
            var redactor = new Redactor();
            var result = redactor.Redact(input);

            //assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [TestCase("{\"woop missin:2324")]
        [TestCase("[\"woop missin:2324")]
        public void RedactJson_ReturnsInput_WhenItsNotValidJson(string input)
        {
            var redactor = new Redactor();
            var result = redactor.Redact(input);

            //assert
            Assert.That(result, Is.EqualTo(input));
        }

        [TestCase("123", "123")]
        [TestCase("{\"pass\":\"password\"}", "{\"pass\":\"**Redacted**\"}")]
        [TestCase("{\"notredacted\":\"value\", \"pass\":\"password\"}", "{\"notredacted\":\"value\", \"pass\":\"**Redacted**\"}")]
        [TestCase("{\"notredacted\":\"value\"}", "{\"notredacted\":\"value\"}")]
        [TestCase("[{\"pass\":\"password\"}]", "[{\"pass\":\"**Redacted**\"}]")]
        [TestCase("[{\"notredacted\":\"value\", \"pass\":\"password\"}]", "[{\"notredacted\":\"value\", \"pass\":\"**Redacted**\"}]")]
        [TestCase("[{\"notredacted\":\"value\"}]", "[{\"notredacted\":\"value\"}]")]
        public void RedactJson_ShouldRedactTopLevelAsExpected(string json, string expected)
        {
            var redactor = new Redactor();
            //act
            var result = redactor.Redact(json);
            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse(expected);
            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true), () => $"result:\r\n{result}\r\n\r\nexpected:\r\n{expected}");
        }

        [Test]
        public void RedactJson_Redacts_WhenPropertyContainsARedactName()
        {
            var redactor = new Redactor();
            var result = redactor.Redact("{\"this is a pass word\":\"password\"}"); //<-- cos it contains pass

            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse("{\"this is a pass word\":\"**Redacted**\"}");
            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true));
        }

        [Test]
        public void RedactJson_Redacts_WhenPropertyIsNotAtRoot()
        {
            var redactor = new Redactor();
            var result = redactor.Redact("{\"root\":{\"level1\":{\"level2\":{\"pass\":\"password\"}}}}");

            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse("{\"root\":{\"level1\":{\"level2\":{\"pass\":\"**Redacted**\"}}}}");
            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true));
        }

        [Test]
        public void RedactJson_TopLevelArray()
        {
            var redactor = new Redactor();
            var result = redactor.Redact("[1,2,3]");

            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse("[1,2,3]");
            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true));
        }

        [TestCase("{\"this.contains.dots.pass\":\"password\"}", "{\"this.contains.dots.pass\":\"**Redacted**\"}")]
        [TestCase("{\"endswithquotepass`\":\"password\"}", "{\"endswithquotepass`\":\"**Redacted**\"}")]
        [TestCase("{\"endswithbracketpass]\":\"password\"}", "{\"endswithbracketpass]\":\"**Redacted**\"}")]
        [TestCase("[{\"this.contains.dots.pass\":\"password\"}]", "[{\"this.contains.dots.pass\":\"**Redacted**\"}]")]
        [TestCase("[{\"endswithquotepass`\":\"password\"}]", "[{\"endswithquotepass`\":\"**Redacted**\"}]")]
        [TestCase("[{\"endswithbracketpass]\":\"password\"}]", "[{\"endswithbracketpass]\":\"**Redacted**\"}]")]
        public void RedactJson_ShouldRedactWhenPropertyContainsCharacters(string json, string expected)
        {
            var redactor = new Redactor();
            //act
            var result = redactor.Redact(json);
            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse(expected);
            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true), () => $"result:\r\n{result}\r\n\r\nexpected:\r\n{expected}");
        }

        [TestCase("authorization")]
        [TestCase("pass")]
        [TestCase("basic")]
        [TestCase("secret")]
        [TestCase("ocp-apim-subscription-key")]
        [TestCase("endpointkey")]
        [TestCase("api-key")]
        [TestCase("apikey")]
        [TestCase("token")]
        public void RedactJson_Redacts_ForPropertyWithName(string propName)
        {
            var redactor = new Redactor();
            var result = redactor.Redact($"{{\"{propName}\":\"password\"}}");

            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse($"{{\"{propName}\":\"**Redacted**\"}}");

            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true));
        }

        [TestCase("TotalTokens")]
        [TestCase("total_tokens")]
        [TestCase("completion_tokens")]
        [TestCase("prompt_tokens")]
        [TestCase("max_tokens")]
        [TestCase("MaxTokens")]
        public void RedactJson_DoesNotRedact_WhenPropertyNameInAllowList(string propName)
        {
            var redactor = new Redactor();
            var result = redactor.Redact($"{{\"{propName}\":\"donotredact\"}}");

            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse($"{{\"{propName}\":\"donotredact\"}}");

            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true));
        }

        [Test]
        public void RedactJson_Redacts_ComplexJson()
        {
            var json = @"{
    ""flow"": {
        ""begin"": {
            ""type"": ""Begin"",
            ""actions"": [
                {
                    ""type"": ""CallProjectFlow"",
                    ""flowRef"": ""ServiceNow.GetValidatedUser"",
                    ""arguments"": [],
                    ""returns"": [
                        {
                            ""returnRef"": ""d00bd921-5c32-403b-9abd-764597d2fb91"",
                            ""assignment"": {
                                ""type"": ""CreateVariable"",
                                ""variable"": {
                                    ""id"": ""abf99f84-a503-4514-9b40-11e2ca91b7e9"",
                                    ""name"": ""serviceNowUser"",
                                    ""scope"": ""Dialog"",
                                    ""variableType"": ""Object""
                                }
                            }
                        }
                    ],
                    ""id"": ""tj-0c709641-c1ac-4c51-8981-963c2a6e2ad7"",
                    ""title"": """"
                },
                {
                    ""type"": ""If"",
                    ""condition"": {
                        ""type"": ""group"",
                        ""logicalOperator"": ""And"",
                        ""expressions"": [
                            {
                                ""type"": ""expression"",
                                ""left"": ""$abf99f84-a503-4514-9b40-11e2ca91b7e9$"",
                                ""operator"": ""HasNotBeenSet"",
                                ""right"": """"
                            }
                        ]
                    },
                    ""then"": [
                        {
                            ""type"": ""Return"",
                            ""values"": [],
                            ""id"": ""tj-b11e98f0-a03f-4f27-b272-184ef6f4976d"",
                            ""title"": """"
                        }
                    ],
                    ""else"": [],
                    ""id"": ""tj-a1d998cf-f465-4373-a0a2-6429338eb9b1"",
                    ""title"": """"
                },
                {
                    ""type"": ""If"",
                    ""condition"": {
                        ""type"": ""group"",
                        ""logicalOperator"": ""And"",
                        ""expressions"": [
                            {
                                ""type"": ""expression"",
                                ""left"": ""$d137d0f4-4313-4892-b399-da23846f2ce9$"",
                                ""operator"": ""HasNotBeenSet"",
                                ""right"": """"
                            }
                        ]
                    },
                    ""then"": [
                        {
                            ""type"": ""AssignVariables"",
                            ""variableAssignments"": [
                                {
                                    ""expression"": ""false"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""d137d0f4-4313-4892-b399-da23846f2ce9"",
                                            ""name"": ""isRegisteredForNotifications"",
                                            ""scope"": ""Conversation"",
                                            ""variableType"": ""Boolean""
                                        }
                                    }
                                }
                            ],
                            ""id"": ""tj-1a97518c-48d6-4d82-a432-e50e40258ade"",
                            ""title"": """"
                        }
                    ],
                    ""else"": [],
                    ""id"": ""tj-09e68e3a-c627-41c8-be45-12f115912235"",
                    ""title"": """"
                },
                {
                    ""type"": ""AssignVariables"",
                    ""variableAssignments"": [
                        {
                            ""expression"": ""[1]"",
                            ""assignment"": {
                                ""type"": ""CreateVariable"",
                                ""variable"": {
                                    ""id"": ""5300177b-3560-4319-a38e-1a2fd454fe52"",
                                    ""name"": ""forever"",
                                    ""scope"": ""Dialog"",
                                    ""variableType"": ""Array""
                                }
                            }
                        }
                    ],
                    ""id"": ""tj-d20ba945-d871-4af8-aae9-c5d294490ddd"",
                    ""title"": """"
                },
                {
                    ""type"": ""ForEach"",
                    ""variableRef"": ""$5300177b-3560-4319-a38e-1a2fd454fe52$"",
                    ""currentItemVariable"": {
                        ""id"": ""19acb333-dad5-43ff-87cc-47a801343613"",
                        ""name"": ""item"",
                        ""scope"": ""Dialog"",
                        ""variableType"": ""Object""
                    },
                    ""currentIndexVariable"": {
                        ""id"": ""7f48b3c4-b294-4c83-824f-01656bb4b8a0"",
                        ""name"": ""index"",
                        ""scope"": ""Dialog"",
                        ""variableType"": ""Number""
                    },
                    ""countVariable"": {
                        ""id"": ""d3d04ddf-0105-43d7-9031-7267bb1f17dc"",
                        ""name"": ""total"",
                        ""scope"": ""Dialog"",
                        ""variableType"": ""Number""
                    },
                    ""actions"": [
                        {
                            ""type"": ""CallProjectFlow"",
                            ""flowRef"": ""ServiceNow.GetIncidents"",
                            ""arguments"": [
                                {
                                    ""paramRef"": ""b24933a0-3fbb-4424-a10c-c02e0a1584f0"",
                                    ""paramVariableType"": ""Object"",
                                    ""value"": ""=${$abf99f84-a503-4514-9b40-11e2ca91b7e9$}\n""
                                },
                                {
                                    ""paramRef"": ""93784b94-1f88-444b-b7e3-31832f3cbdf4"",
                                    ""paramVariableType"": ""Number"",
                                    ""value"": ""=${$7f48b3c4-b294-4c83-824f-01656bb4b8a0$} + 1""
                                }
                            ],
                            ""returns"": [
                                {
                                    ""returnRef"": ""d3431b79-5948-41db-ab47-18bb3feb9f7b"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""a091ea6f-bc84-4f32-a08f-1f45798fd7b2"",
                                            ""name"": ""incidents"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""Array""
                                        }
                                    }
                                },
                                {
                                    ""returnRef"": ""71152b08-5d01-4c81-8302-3f18e71a31e0"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""fd2cc6a4-aa74-4090-8340-54214a74a691"",
                                            ""name"": ""totalIncidents"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""Number""
                                        }
                                    }
                                },
                                {
                                    ""returnRef"": ""2cf9c896-12f5-4a0c-8733-7e7c4752a75d"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""ee37ed61-6731-4bd5-8f70-f232a1d8981e"",
                                            ""name"": ""hasMorePages"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""Boolean""
                                        }
                                    }
                                }
                            ],
                            ""id"": ""tj-357c65c7-ffa2-4447-bab4-bcd5bbb4885d"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""AssignVariables"",
                            ""variableAssignments"": [
                                {
                                    ""expression"": """",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""72919745-3c3b-4c4e-bdfa-e86afdd32f03"",
                                            ""name"": ""intentChoice"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""String""
                                        }
                                    }
                                }
                            ],
                            ""id"": ""tj-7be772f5-20a6-4d73-b0cb-0e01b2f6df5c"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""AssignVariables"",
                            ""variableAssignments"": [
                                {
                                    ""expression"": ""[{text:\""Done\"", intent:\""Done\"", password:\""123\""}]"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""0561285f-e4de-4106-a790-9f98ce143575"",
                                            ""name"": ""staticChoices"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""Array""
                                        }
                                    }
                                }
                            ],
                            ""id"": ""tj-81423f82-64fc-4d39-95ab-04bbecf5ecc5"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""If"",
                            ""condition"": {
                                ""type"": ""group"",
                                ""logicalOperator"": ""And"",
                                ""expressions"": [
                                    {
                                        ""type"": ""expression"",
                                        ""left"": ""$d137d0f4-4313-4892-b399-da23846f2ce9$"",
                                        ""operator"": ""IsTrue"",
                                        ""right"": """"
                                    }
                                ]
                            },
                            ""then"": [
                                {
                                    ""type"": ""AssignVariables"",
                                    ""variableAssignments"": [
                                        {
                                            ""expression"": ""=concat([{text:\""Stop keeping me updated\"", intent:\""UnregisterForNotification\""}],${$0561285f-e4de-4106-a790-9f98ce143575$})"",
                                            ""assignment"": {
                                                ""type"": ""UpdateVariable"",
                                                ""variableRef"": ""$0561285f-e4de-4106-a790-9f98ce143575$""
                                            }
                                        }
                                    ],
                                    ""id"": ""tj-fb51d523-cb41-4ad1-b9b0-a60b3df3ca57"",
                                    ""title"": """"
                                }
                            ],
                            ""else"": [
                                {
                                    ""type"": ""AssignVariables"",
                                    ""variableAssignments"": [
                                        {
                                            ""expression"": ""=concat([{text:\""Keep me updated\"", intent:\""RegisterForNotification\""}],${$0561285f-e4de-4106-a790-9f98ce143575$})"",
                                            ""assignment"": {
                                                ""type"": ""UpdateVariable"",
                                                ""variableRef"": ""$0561285f-e4de-4106-a790-9f98ce143575$""
                                            }
                                        }
                                    ],
                                    ""id"": ""tj-c5ea8851-2237-4664-a73f-94f0593b55de"",
                                    ""title"": """"
                                }
                            ],
                            ""id"": ""tj-0e3d2f04-2f56-45d3-a33f-5201aea21f16"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""If"",
                            ""condition"": {
                                ""type"": ""group"",
                                ""logicalOperator"": ""And"",
                                ""expressions"": [
                                    {
                                        ""type"": ""expression"",
                                        ""left"": ""$ee37ed61-6731-4bd5-8f70-f232a1d8981e$"",
                                        ""operator"": ""IsTrue"",
                                        ""right"": """"
                                    }
                                ]
                            },
                            ""then"": [
                                {
                                    ""type"": ""AssignVariables"",
                                    ""variableAssignments"": [
                                        {
                                            ""expression"": ""=concat([{text:\""More\"", intent:\""More\""}],${$0561285f-e4de-4106-a790-9f98ce143575$})"",
                                            ""assignment"": {
                                                ""type"": ""UpdateVariable"",
                                                ""variableRef"": ""$0561285f-e4de-4106-a790-9f98ce143575$""
                                            },
                                            ""password"": ""Top Secret""
                                        }
                                    ],
                                    ""id"": ""tj-66956e78-e09f-4ee2-8490-c16982d076e8"",
                                    ""title"": """"
                                }
                            ],
                            ""else"": [],
                            ""id"": ""tj-7a2093ce-60db-4a2d-8db4-5ef11af49fbd"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""Question"",
                            ""questionText"": """",
                            ""inputType"": {
                                ""type"": ""Choice"",
                                ""inputChoices"": [
                                    {
                                        ""type"": ""ChoiceOptionData"",
                                        ""variableRef"": ""$a091ea6f-bc84-4f32-a08f-1f45798fd7b2$"",
                                        ""jsonPathText"": ""$.ShortDescription"",
                                        ""jsonPathIntent"": ""$.SysId"",
                                        ""id"": ""7c0e0c76-8313-4f1a-b330-f590fb81b41f""
                                    },
                                    {
                                        ""type"": ""ChoiceOptionData"",
                                        ""variableRef"": ""$0561285f-e4de-4106-a790-9f98ce143575$"",
                                        ""jsonPathText"": ""$.text"",
                                        ""jsonPathIntent"": ""$.intent"",
                                        ""id"": ""0bde0f70-03a1-4275-88f0-29fa2419e476""
                                    }
                                ],
                                ""acceptUnmatchedResponse"": false,
                                ""choices"": []
                            },
                            ""variableAssignment"": {
                                ""type"": ""UpdateVariable"",
                                ""variableRef"": ""$72919745-3c3b-4c4e-bdfa-e86afdd32f03$""
                            },
                            ""id"": ""tj-746adcab-1a59-4418-ba32-917876d79d88"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""Switch"",
                            ""conditionVariableRef"": ""$72919745-3c3b-4c4e-bdfa-e86afdd32f03$"",
                            ""cases"": [
                                {
                                    ""id"": ""9855b3af-84aa-4317-9fe4-26aa3a9fa760"",
                                    ""value"": ""Done"",
                                    ""actions"": [
                                        {
                                            ""type"": ""BreakLoop"",
                                            ""id"": ""tj-7cd6054f-5615-4e3b-bd82-834c61e08514"",
                                            ""title"": """"
                                        }
                                    ]
                                },
                                {
                                    ""id"": ""1db758bc-ab4f-49f5-9177-aff4786c4ce1"",
                                    ""value"": ""More"",
                                    ""actions"": []
                                },
                                {
                                    ""id"": ""c578b67c-65d0-47a6-b04f-e025c343dc71"",
                                    ""value"": ""RegisterForNotification"",
                                    ""actions"": [
                                        {
                                            ""type"": ""ServiceNowRegisterForNotifications"",
                                            ""serviceNowUserVariableRef"": ""$abf99f84-a503-4514-9b40-11e2ca91b7e9$"",
                                            ""id"": ""tj-fa89481f-a99d-4948-a467-9065aaa16372"",
                                            ""title"": """"
                                        },
                                        {
                                            ""type"": ""AssignVariables"",
                                            ""variableAssignments"": [
                                                {
                                                    ""expression"": ""true"",
                                                    ""assignment"": {
                                                        ""type"": ""UpdateVariable"",
                                                        ""variableRef"": ""$d137d0f4-4313-4892-b399-da23846f2ce9$""
                                                    }
                                                }
                                            ],
                                            ""id"": ""tj-192c7f48-ee4e-4190-931e-4f5b78c17e87"",
                                            ""title"": """"
                                        },
                                        {
                                            ""type"": ""SendMessage"",
                                            ""message"": ""Ok, I'll keep you updated"",
                                            ""id"": ""tj-43bdcf13-62d3-4a9a-9423-0c7171ebdaf1"",
                                            ""title"": """"
                                        }
                                    ]
                                },
                                {
                                    ""id"": ""774663eb-1882-4c72-bea6-047b728bfaf8"",
                                    ""value"": ""UnregisterForNotification"",
                                    ""actions"": [
                                        {
                                            ""type"": ""ServiceNowUnRegisterForNotifications"",
                                            ""serviceNowUserVariableRef"": ""$abf99f84-a503-4514-9b40-11e2ca91b7e9$"",
                                            ""id"": ""tj-74b0346d-d6e7-4fc2-b0d3-915cb7133148"",
                                            ""title"": """"
                                        },
                                        {
                                            ""type"": ""AssignVariables"",
                                            ""variableAssignments"": [
                                                {
                                                    ""expression"": ""false"",
                                                    ""assignment"": {
                                                        ""type"": ""UpdateVariable"",
                                                        ""variableRef"": ""$d137d0f4-4313-4892-b399-da23846f2ce9$""
                                                    }
                                                }
                                            ],
                                            ""id"": ""tj-16efdee5-d2ad-4b58-b11b-fc657675f136"",
                                            ""title"": """"
                                        },
                                        {
                                            ""type"": ""SendMessage"",
                                            ""message"": ""Ok, I'll stop updating you"",
                                            ""id"": ""tj-884e1dfc-a60f-4021-99ef-7e92ad79be6c"",
                                            ""title"": """"
                                        }
                                    ]
                                }
                            ],
                            ""defaultActions"": [
                                {
                                    ""type"": ""AssignVariables"",
                                    ""variableAssignments"": [
                                        {
                                            ""expression"": ""=${$72919745-3c3b-4c4e-bdfa-e86afdd32f03$}\n"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""412f41ee-80fe-4550-89d7-7ff30eb45423"",
                                                    ""name"": ""sysId"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Object""
                                                }
                                            }
                                        }
                                    ],
                                    ""id"": ""tj-69b58539-4ab8-41e2-881b-ddb866600d60"",
                                    ""title"": """"
                                },
                                {
                                    ""type"": ""CallProjectFlow"",
                                    ""flowRef"": ""ServiceNow.GetIncidentComments"",
                                    ""arguments"": [
                                        {
                                            ""paramRef"": ""b24933a0-3fbb-4424-a10c-c02e0a1584f0"",
                                            ""paramVariableType"": ""Object"",
                                            ""value"": ""=${$abf99f84-a503-4514-9b40-11e2ca91b7e9$}\n""
                                        },
                                        {
                                            ""paramRef"": ""93784b94-1f88-444b-b7e3-31832f3cbdf4"",
                                            ""paramVariableType"": ""Number"",
                                            ""value"": ""1""
                                        },
                                        {
                                            ""paramRef"": ""e6b1ad0f-ec0b-4adf-bd91-47590447e833"",
                                            ""paramVariableType"": ""String"",
                                            ""value"": ""=${$412f41ee-80fe-4550-89d7-7ff30eb45423$}""
                                        }
                                    ],
                                    ""returns"": [
                                        {
                                            ""returnRef"": ""d3431b79-5948-41db-ab47-18bb3feb9f7b"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""834932f7-1b87-400e-bac4-591af82457b8"",
                                                    ""name"": ""comments"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Array""
                                                }
                                            }
                                        },
                                        {
                                            ""returnRef"": ""71152b08-5d01-4c81-8302-3f18e71a31e0"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""5fb69a6d-9828-46cc-bd03-e5be38cae954"",
                                                    ""name"": ""totalComments"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Number""
                                                }
                                            }
                                        },
                                        {
                                            ""returnRef"": ""2cf9c896-12f5-4a0c-8733-7e7c4752a75d"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""fc8b7362-822c-4867-a824-8640ce8f68fa"",
                                                    ""name"": ""hasMoreCommentPages"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Boolean""
                                                }
                                            }
                                        },
                                        {
                                            ""returnRef"": ""486ab700-83bb-4626-bc61-afe2db8a53bd"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""cef1d1aa-e2ee-4291-8ebc-cecd3123aa41"",
                                                    ""name"": ""totalPages"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Number""
                                                }
                                            }
                                        }
                                    ],
                                    ""id"": ""tj-a995a743-8b23-4cd8-9dea-7b1b361db3aa"",
                                    ""title"": """"
                                },
                                {
                                    ""type"": ""CallProjectFlow"",
                                    ""flowRef"": ""ServiceNow.ShowIncident"",
                                    ""arguments"": [
                                        {
                                            ""paramRef"": ""1e66c7ef-c2b8-47d1-ae59-4b2b696f6f3b"",
                                            ""paramVariableType"": ""Object"",
                                            ""value"": ""=first(where(${$a091ea6f-bc84-4f32-a08f-1f45798fd7b2$}, i => i.sysId == ${$412f41ee-80fe-4550-89d7-7ff30eb45423$}))\n""
                                        },
                                        {
                                            ""paramRef"": ""6f16dcbb-a048-4128-a443-5d80ccfaf376"",
                                            ""paramVariableType"": ""Object"",
                                            ""value"": ""=first(${$834932f7-1b87-400e-bac4-591af82457b8$})\n""
                                        },
                                        {
                                            ""paramRef"": ""09f3c595-ddec-4a0e-a295-4d2b221fa282"",
                                            ""paramVariableType"": ""Number"",
                                            ""value"": ""=${$5fb69a6d-9828-46cc-bd03-e5be38cae954$}""
                                        }
                                    ],
                                    ""returns"": [],
                                    ""id"": ""tj-7f22fce4-300d-4bc6-bb35-f1a800f02561"",
                                    ""title"": """"
                                },
                                {
                                    ""type"": ""Question"",
                                    ""questionText"": """",
                                    ""inputType"": {
                                        ""type"": ""Choice"",
                                        ""inputChoices"": [
                                            {
                                                ""type"": ""ChoiceOptionStatic"",
                                                ""text"": ""Add Comment"",
                                                ""intent"": ""addcomment"",
                                                ""id"": ""0a46be03-a300-46a6-957d-f530b72536df""
                                            },
                                            {
                                                ""type"": ""ChoiceOptionStatic"",
                                                ""text"": ""View Comments"",
                                                ""intent"": ""viewcomments"",
                                                ""id"": ""4424728c-41b4-4aa7-8c5b-00d248c5f818""
                                            },
                                            {
                                                ""type"": ""ChoiceOptionStatic"",
                                                ""text"": ""Done"",
                                                ""intent"": ""done"",
                                                ""id"": ""a95eaadf-4520-4488-bb7a-791ae42b4731""
                                            }
                                        ],
                                        ""acceptUnmatchedResponse"": false,
                                        ""choices"": []
                                    },
                                    ""variableAssignment"": {
                                        ""type"": ""UpdateVariable"",
                                        ""variableRef"": ""$72919745-3c3b-4c4e-bdfa-e86afdd32f03$""
                                    },
                                    ""id"": ""tj-bd0f8179-47ab-4e63-bc61-081b9737b73a"",
                                    ""title"": """"
                                },
                                {
                                    ""type"": ""Switch"",
                                    ""conditionVariableRef"": ""$72919745-3c3b-4c4e-bdfa-e86afdd32f03$"",
                                    ""cases"": [
                                        {
                                            ""id"": ""136b1e06-76f8-40e6-a031-f0834a9c67db"",
                                            ""value"": ""viewcomments"",
                                            ""actions"": [
                                                {
                                                    ""type"": ""CallProjectFlow"",
                                                    ""flowRef"": ""ServiceNow.ShowIncidentComments"",
                                                    ""arguments"": [
                                                        {
                                                            ""paramRef"": ""067b45ac-b44a-4f8e-a165-bafac66d07b8"",
                                                            ""paramVariableType"": ""Object"",
                                                            ""value"": ""=${$abf99f84-a503-4514-9b40-11e2ca91b7e9$}\n""
                                                        },
                                                        {
                                                            ""paramRef"": ""1ed9fbe4-cda9-4395-9041-7e5eb9d6e214"",
                                                            ""paramVariableType"": ""Object"",
                                                            ""value"": ""=first(where(${$a091ea6f-bc84-4f32-a08f-1f45798fd7b2$}, i => i.sysId == ${$412f41ee-80fe-4550-89d7-7ff30eb45423$}))\n""
                                                        }
                                                    ],
                                                    ""returns"": [],
                                                    ""id"": ""tj-faf121c7-bf45-4255-82fd-445d4feb4116"",
                                                    ""title"": """"
                                                }
                                            ]
                                        },
                                        {
                                            ""id"": ""3beabbb8-fa96-4376-b2b8-ce43f3884dee"",
                                            ""value"": ""addcomment"",
                                            ""actions"": [
                                                {
                                                    ""type"": ""CallProjectFlow"",
                                                    ""flowRef"": ""ServiceNow.AddComment"",
                                                    ""arguments"": [
                                                        {
                                                            ""paramRef"": ""ba85b433-40fa-4763-9a9c-fbff1fc44a3b"",
                                                            ""paramVariableType"": ""Object"",
                                                            ""value"": ""=${$abf99f84-a503-4514-9b40-11e2ca91b7e9$}\n""
                                                        },
                                                        {
                                                            ""paramRef"": ""93486419-a519-4d08-901b-f7cf0c9c8a55"",
                                                            ""paramVariableType"": ""Object"",
                                                            ""value"": ""=first(where(${$a091ea6f-bc84-4f32-a08f-1f45798fd7b2$}, i => i.sysId == ${$412f41ee-80fe-4550-89d7-7ff30eb45423$}))\n""
                                                        }
                                                    ],
                                                    ""returns"": [],
                                                    ""id"": ""tj-21be3725-6303-461d-a512-1654cabbe441"",
                                                    ""title"": """"
                                                }
                                            ]
                                        }
                                    ],
                                    ""defaultActions"": [],
                                    ""id"": ""tj-6763e272-6004-47bb-914e-d2c8172f217a"",
                                    ""title"": """"
                                }
                            ],
                            ""id"": ""tj-9c3abce2-4e84-4e48-acaf-e186f4a2d7e0"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""AssignVariables"",
                            ""variableAssignments"": [
                                {
                                    ""expression"": ""= concat(${$5300177b-3560-4319-a38e-1a2fd454fe52$}, [1])"",
                                    ""assignment"": {
                                        ""type"": ""UpdateVariable"",
                                        ""variableRef"": ""$5300177b-3560-4319-a38e-1a2fd454fe52$""
                                    }
                                }
                            ],
                            ""id"": ""tj-03338505-e9b1-4644-ba32-8ce3b9e3afd9"",
                            ""title"": """"
                        }
                    ],
                    ""id"": ""tj-2bb97ddc-5ef3-4eb3-9288-ef9f39d323d9"",
                    ""title"": """"
                }
            ]
        },
        ""inputParameters"": [],
        ""returnParameters"": []
    }
}";
            var redactedJson = @"{
    ""flow"": {
        ""begin"": {
            ""type"": ""Begin"",
            ""actions"": [
                {
                    ""type"": ""CallProjectFlow"",
                    ""flowRef"": ""ServiceNow.GetValidatedUser"",
                    ""arguments"": [],
                    ""returns"": [
                        {
                            ""returnRef"": ""d00bd921-5c32-403b-9abd-764597d2fb91"",
                            ""assignment"": {
                                ""type"": ""CreateVariable"",
                                ""variable"": {
                                    ""id"": ""abf99f84-a503-4514-9b40-11e2ca91b7e9"",
                                    ""name"": ""serviceNowUser"",
                                    ""scope"": ""Dialog"",
                                    ""variableType"": ""Object""
                                }
                            }
                        }
                    ],
                    ""id"": ""tj-0c709641-c1ac-4c51-8981-963c2a6e2ad7"",
                    ""title"": """"
                },
                {
                    ""type"": ""If"",
                    ""condition"": {
                        ""type"": ""group"",
                        ""logicalOperator"": ""And"",
                        ""expressions"": [
                            {
                                ""type"": ""expression"",
                                ""left"": ""$abf99f84-a503-4514-9b40-11e2ca91b7e9$"",
                                ""operator"": ""HasNotBeenSet"",
                                ""right"": """"
                            }
                        ]
                    },
                    ""then"": [
                        {
                            ""type"": ""Return"",
                            ""values"": [],
                            ""id"": ""tj-b11e98f0-a03f-4f27-b272-184ef6f4976d"",
                            ""title"": """"
                        }
                    ],
                    ""else"": [],
                    ""id"": ""tj-a1d998cf-f465-4373-a0a2-6429338eb9b1"",
                    ""title"": """"
                },
                {
                    ""type"": ""If"",
                    ""condition"": {
                        ""type"": ""group"",
                        ""logicalOperator"": ""And"",
                        ""expressions"": [
                            {
                                ""type"": ""expression"",
                                ""left"": ""$d137d0f4-4313-4892-b399-da23846f2ce9$"",
                                ""operator"": ""HasNotBeenSet"",
                                ""right"": """"
                            }
                        ]
                    },
                    ""then"": [
                        {
                            ""type"": ""AssignVariables"",
                            ""variableAssignments"": [
                                {
                                    ""expression"": ""false"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""d137d0f4-4313-4892-b399-da23846f2ce9"",
                                            ""name"": ""isRegisteredForNotifications"",
                                            ""scope"": ""Conversation"",
                                            ""variableType"": ""Boolean""
                                        }
                                    }
                                }
                            ],
                            ""id"": ""tj-1a97518c-48d6-4d82-a432-e50e40258ade"",
                            ""title"": """"
                        }
                    ],
                    ""else"": [],
                    ""id"": ""tj-09e68e3a-c627-41c8-be45-12f115912235"",
                    ""title"": """"
                },
                {
                    ""type"": ""AssignVariables"",
                    ""variableAssignments"": [
                        {
                            ""expression"": ""[1]"",
                            ""assignment"": {
                                ""type"": ""CreateVariable"",
                                ""variable"": {
                                    ""id"": ""5300177b-3560-4319-a38e-1a2fd454fe52"",
                                    ""name"": ""forever"",
                                    ""scope"": ""Dialog"",
                                    ""variableType"": ""Array""
                                }
                            }
                        }
                    ],
                    ""id"": ""tj-d20ba945-d871-4af8-aae9-c5d294490ddd"",
                    ""title"": """"
                },
                {
                    ""type"": ""ForEach"",
                    ""variableRef"": ""$5300177b-3560-4319-a38e-1a2fd454fe52$"",
                    ""currentItemVariable"": {
                        ""id"": ""19acb333-dad5-43ff-87cc-47a801343613"",
                        ""name"": ""item"",
                        ""scope"": ""Dialog"",
                        ""variableType"": ""Object""
                    },
                    ""currentIndexVariable"": {
                        ""id"": ""7f48b3c4-b294-4c83-824f-01656bb4b8a0"",
                        ""name"": ""index"",
                        ""scope"": ""Dialog"",
                        ""variableType"": ""Number""
                    },
                    ""countVariable"": {
                        ""id"": ""d3d04ddf-0105-43d7-9031-7267bb1f17dc"",
                        ""name"": ""total"",
                        ""scope"": ""Dialog"",
                        ""variableType"": ""Number""
                    },
                    ""actions"": [
                        {
                            ""type"": ""CallProjectFlow"",
                            ""flowRef"": ""ServiceNow.GetIncidents"",
                            ""arguments"": [
                                {
                                    ""paramRef"": ""b24933a0-3fbb-4424-a10c-c02e0a1584f0"",
                                    ""paramVariableType"": ""Object"",
                                    ""value"": ""=${$abf99f84-a503-4514-9b40-11e2ca91b7e9$}\n""
                                },
                                {
                                    ""paramRef"": ""93784b94-1f88-444b-b7e3-31832f3cbdf4"",
                                    ""paramVariableType"": ""Number"",
                                    ""value"": ""=${$7f48b3c4-b294-4c83-824f-01656bb4b8a0$} + 1""
                                }
                            ],
                            ""returns"": [
                                {
                                    ""returnRef"": ""d3431b79-5948-41db-ab47-18bb3feb9f7b"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""a091ea6f-bc84-4f32-a08f-1f45798fd7b2"",
                                            ""name"": ""incidents"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""Array""
                                        }
                                    }
                                },
                                {
                                    ""returnRef"": ""71152b08-5d01-4c81-8302-3f18e71a31e0"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""fd2cc6a4-aa74-4090-8340-54214a74a691"",
                                            ""name"": ""totalIncidents"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""Number""
                                        }
                                    }
                                },
                                {
                                    ""returnRef"": ""2cf9c896-12f5-4a0c-8733-7e7c4752a75d"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""ee37ed61-6731-4bd5-8f70-f232a1d8981e"",
                                            ""name"": ""hasMorePages"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""Boolean""
                                        }
                                    }
                                }
                            ],
                            ""id"": ""tj-357c65c7-ffa2-4447-bab4-bcd5bbb4885d"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""AssignVariables"",
                            ""variableAssignments"": [
                                {
                                    ""expression"": """",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""72919745-3c3b-4c4e-bdfa-e86afdd32f03"",
                                            ""name"": ""intentChoice"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""String""
                                        }
                                    }
                                }
                            ],
                            ""id"": ""tj-7be772f5-20a6-4d73-b0cb-0e01b2f6df5c"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""AssignVariables"",
                            ""variableAssignments"": [
                                {
                                    ""expression"": ""[{\""text\"":\""Done\"",\""intent\"":\""Done\"",\""password\"":\""**Redacted**\""}]"",
                                    ""assignment"": {
                                        ""type"": ""CreateVariable"",
                                        ""variable"": {
                                            ""id"": ""0561285f-e4de-4106-a790-9f98ce143575"",
                                            ""name"": ""staticChoices"",
                                            ""scope"": ""Dialog"",
                                            ""variableType"": ""Array""
                                        }
                                    }
                                }
                            ],
                            ""id"": ""tj-81423f82-64fc-4d39-95ab-04bbecf5ecc5"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""If"",
                            ""condition"": {
                                ""type"": ""group"",
                                ""logicalOperator"": ""And"",
                                ""expressions"": [
                                    {
                                        ""type"": ""expression"",
                                        ""left"": ""$d137d0f4-4313-4892-b399-da23846f2ce9$"",
                                        ""operator"": ""IsTrue"",
                                        ""right"": """"
                                    }
                                ]
                            },
                            ""then"": [
                                {
                                    ""type"": ""AssignVariables"",
                                    ""variableAssignments"": [
                                        {
                                            ""expression"": ""=concat([{text:\""Stop keeping me updated\"", intent:\""UnregisterForNotification\""}],${$0561285f-e4de-4106-a790-9f98ce143575$})"",
                                            ""assignment"": {
                                                ""type"": ""UpdateVariable"",
                                                ""variableRef"": ""$0561285f-e4de-4106-a790-9f98ce143575$""
                                            }
                                        }
                                    ],
                                    ""id"": ""tj-fb51d523-cb41-4ad1-b9b0-a60b3df3ca57"",
                                    ""title"": """"
                                }
                            ],
                            ""else"": [
                                {
                                    ""type"": ""AssignVariables"",
                                    ""variableAssignments"": [
                                        {
                                            ""expression"": ""=concat([{text:\""Keep me updated\"", intent:\""RegisterForNotification\""}],${$0561285f-e4de-4106-a790-9f98ce143575$})"",
                                            ""assignment"": {
                                                ""type"": ""UpdateVariable"",
                                                ""variableRef"": ""$0561285f-e4de-4106-a790-9f98ce143575$""
                                            }
                                        }
                                    ],
                                    ""id"": ""tj-c5ea8851-2237-4664-a73f-94f0593b55de"",
                                    ""title"": """"
                                }
                            ],
                            ""id"": ""tj-0e3d2f04-2f56-45d3-a33f-5201aea21f16"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""If"",
                            ""condition"": {
                                ""type"": ""group"",
                                ""logicalOperator"": ""And"",
                                ""expressions"": [
                                    {
                                        ""type"": ""expression"",
                                        ""left"": ""$ee37ed61-6731-4bd5-8f70-f232a1d8981e$"",
                                        ""operator"": ""IsTrue"",
                                        ""right"": """"
                                    }
                                ]
                            },
                            ""then"": [
                                {
                                    ""type"": ""AssignVariables"",
                                    ""variableAssignments"": [
                                        {
                                            ""expression"": ""=concat([{text:\""More\"", intent:\""More\""}],${$0561285f-e4de-4106-a790-9f98ce143575$})"",
                                            ""assignment"": {
                                                ""type"": ""UpdateVariable"",
                                                ""variableRef"": ""$0561285f-e4de-4106-a790-9f98ce143575$""
                                            },
                                            ""password"": ""**Redacted**""
                                        }
                                    ],
                                    ""id"": ""tj-66956e78-e09f-4ee2-8490-c16982d076e8"",
                                    ""title"": """"
                                }
                            ],
                            ""else"": [],
                            ""id"": ""tj-7a2093ce-60db-4a2d-8db4-5ef11af49fbd"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""Question"",
                            ""questionText"": """",
                            ""inputType"": {
                                ""type"": ""Choice"",
                                ""inputChoices"": [
                                    {
                                        ""type"": ""ChoiceOptionData"",
                                        ""variableRef"": ""$a091ea6f-bc84-4f32-a08f-1f45798fd7b2$"",
                                        ""jsonPathText"": ""$.ShortDescription"",
                                        ""jsonPathIntent"": ""$.SysId"",
                                        ""id"": ""7c0e0c76-8313-4f1a-b330-f590fb81b41f""
                                    },
                                    {
                                        ""type"": ""ChoiceOptionData"",
                                        ""variableRef"": ""$0561285f-e4de-4106-a790-9f98ce143575$"",
                                        ""jsonPathText"": ""$.text"",
                                        ""jsonPathIntent"": ""$.intent"",
                                        ""id"": ""0bde0f70-03a1-4275-88f0-29fa2419e476""
                                    }
                                ],
                                ""acceptUnmatchedResponse"": false,
                                ""choices"": []
                            },
                            ""variableAssignment"": {
                                ""type"": ""UpdateVariable"",
                                ""variableRef"": ""$72919745-3c3b-4c4e-bdfa-e86afdd32f03$""
                            },
                            ""id"": ""tj-746adcab-1a59-4418-ba32-917876d79d88"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""Switch"",
                            ""conditionVariableRef"": ""$72919745-3c3b-4c4e-bdfa-e86afdd32f03$"",
                            ""cases"": [
                                {
                                    ""id"": ""9855b3af-84aa-4317-9fe4-26aa3a9fa760"",
                                    ""value"": ""Done"",
                                    ""actions"": [
                                        {
                                            ""type"": ""BreakLoop"",
                                            ""id"": ""tj-7cd6054f-5615-4e3b-bd82-834c61e08514"",
                                            ""title"": """"
                                        }
                                    ]
                                },
                                {
                                    ""id"": ""1db758bc-ab4f-49f5-9177-aff4786c4ce1"",
                                    ""value"": ""More"",
                                    ""actions"": []
                                },
                                {
                                    ""id"": ""c578b67c-65d0-47a6-b04f-e025c343dc71"",
                                    ""value"": ""RegisterForNotification"",
                                    ""actions"": [
                                        {
                                            ""type"": ""ServiceNowRegisterForNotifications"",
                                            ""serviceNowUserVariableRef"": ""$abf99f84-a503-4514-9b40-11e2ca91b7e9$"",
                                            ""id"": ""tj-fa89481f-a99d-4948-a467-9065aaa16372"",
                                            ""title"": """"
                                        },
                                        {
                                            ""type"": ""AssignVariables"",
                                            ""variableAssignments"": [
                                                {
                                                    ""expression"": ""true"",
                                                    ""assignment"": {
                                                        ""type"": ""UpdateVariable"",
                                                        ""variableRef"": ""$d137d0f4-4313-4892-b399-da23846f2ce9$""
                                                    }
                                                }
                                            ],
                                            ""id"": ""tj-192c7f48-ee4e-4190-931e-4f5b78c17e87"",
                                            ""title"": """"
                                        },
                                        {
                                            ""type"": ""SendMessage"",
                                            ""message"": ""Ok, I'll keep you updated"",
                                            ""id"": ""tj-43bdcf13-62d3-4a9a-9423-0c7171ebdaf1"",
                                            ""title"": """"
                                        }
                                    ]
                                },
                                {
                                    ""id"": ""774663eb-1882-4c72-bea6-047b728bfaf8"",
                                    ""value"": ""UnregisterForNotification"",
                                    ""actions"": [
                                        {
                                            ""type"": ""ServiceNowUnRegisterForNotifications"",
                                            ""serviceNowUserVariableRef"": ""$abf99f84-a503-4514-9b40-11e2ca91b7e9$"",
                                            ""id"": ""tj-74b0346d-d6e7-4fc2-b0d3-915cb7133148"",
                                            ""title"": """"
                                        },
                                        {
                                            ""type"": ""AssignVariables"",
                                            ""variableAssignments"": [
                                                {
                                                    ""expression"": ""false"",
                                                    ""assignment"": {
                                                        ""type"": ""UpdateVariable"",
                                                        ""variableRef"": ""$d137d0f4-4313-4892-b399-da23846f2ce9$""
                                                    }
                                                }
                                            ],
                                            ""id"": ""tj-16efdee5-d2ad-4b58-b11b-fc657675f136"",
                                            ""title"": """"
                                        },
                                        {
                                            ""type"": ""SendMessage"",
                                            ""message"": ""Ok, I'll stop updating you"",
                                            ""id"": ""tj-884e1dfc-a60f-4021-99ef-7e92ad79be6c"",
                                            ""title"": """"
                                        }
                                    ]
                                }
                            ],
                            ""defaultActions"": [
                                {
                                    ""type"": ""AssignVariables"",
                                    ""variableAssignments"": [
                                        {
                                            ""expression"": ""=${$72919745-3c3b-4c4e-bdfa-e86afdd32f03$}\n"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""412f41ee-80fe-4550-89d7-7ff30eb45423"",
                                                    ""name"": ""sysId"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Object""
                                                }
                                            }
                                        }
                                    ],
                                    ""id"": ""tj-69b58539-4ab8-41e2-881b-ddb866600d60"",
                                    ""title"": """"
                                },
                                {
                                    ""type"": ""CallProjectFlow"",
                                    ""flowRef"": ""ServiceNow.GetIncidentComments"",
                                    ""arguments"": [
                                        {
                                            ""paramRef"": ""b24933a0-3fbb-4424-a10c-c02e0a1584f0"",
                                            ""paramVariableType"": ""Object"",
                                            ""value"": ""=${$abf99f84-a503-4514-9b40-11e2ca91b7e9$}\n""
                                        },
                                        {
                                            ""paramRef"": ""93784b94-1f88-444b-b7e3-31832f3cbdf4"",
                                            ""paramVariableType"": ""Number"",
                                            ""value"": ""1""
                                        },
                                        {
                                            ""paramRef"": ""e6b1ad0f-ec0b-4adf-bd91-47590447e833"",
                                            ""paramVariableType"": ""String"",
                                            ""value"": ""=${$412f41ee-80fe-4550-89d7-7ff30eb45423$}""
                                        }
                                    ],
                                    ""returns"": [
                                        {
                                            ""returnRef"": ""d3431b79-5948-41db-ab47-18bb3feb9f7b"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""834932f7-1b87-400e-bac4-591af82457b8"",
                                                    ""name"": ""comments"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Array""
                                                }
                                            }
                                        },
                                        {
                                            ""returnRef"": ""71152b08-5d01-4c81-8302-3f18e71a31e0"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""5fb69a6d-9828-46cc-bd03-e5be38cae954"",
                                                    ""name"": ""totalComments"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Number""
                                                }
                                            }
                                        },
                                        {
                                            ""returnRef"": ""2cf9c896-12f5-4a0c-8733-7e7c4752a75d"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""fc8b7362-822c-4867-a824-8640ce8f68fa"",
                                                    ""name"": ""hasMoreCommentPages"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Boolean""
                                                }
                                            }
                                        },
                                        {
                                            ""returnRef"": ""486ab700-83bb-4626-bc61-afe2db8a53bd"",
                                            ""assignment"": {
                                                ""type"": ""CreateVariable"",
                                                ""variable"": {
                                                    ""id"": ""cef1d1aa-e2ee-4291-8ebc-cecd3123aa41"",
                                                    ""name"": ""totalPages"",
                                                    ""scope"": ""Dialog"",
                                                    ""variableType"": ""Number""
                                                }
                                            }
                                        }
                                    ],
                                    ""id"": ""tj-a995a743-8b23-4cd8-9dea-7b1b361db3aa"",
                                    ""title"": """"
                                },
                                {
                                    ""type"": ""CallProjectFlow"",
                                    ""flowRef"": ""ServiceNow.ShowIncident"",
                                    ""arguments"": [
                                        {
                                            ""paramRef"": ""1e66c7ef-c2b8-47d1-ae59-4b2b696f6f3b"",
                                            ""paramVariableType"": ""Object"",
                                            ""value"": ""=first(where(${$a091ea6f-bc84-4f32-a08f-1f45798fd7b2$}, i => i.sysId == ${$412f41ee-80fe-4550-89d7-7ff30eb45423$}))\n""
                                        },
                                        {
                                            ""paramRef"": ""6f16dcbb-a048-4128-a443-5d80ccfaf376"",
                                            ""paramVariableType"": ""Object"",
                                            ""value"": ""=first(${$834932f7-1b87-400e-bac4-591af82457b8$})\n""
                                        },
                                        {
                                            ""paramRef"": ""09f3c595-ddec-4a0e-a295-4d2b221fa282"",
                                            ""paramVariableType"": ""Number"",
                                            ""value"": ""=${$5fb69a6d-9828-46cc-bd03-e5be38cae954$}""
                                        }
                                    ],
                                    ""returns"": [],
                                    ""id"": ""tj-7f22fce4-300d-4bc6-bb35-f1a800f02561"",
                                    ""title"": """"
                                },
                                {
                                    ""type"": ""Question"",
                                    ""questionText"": """",
                                    ""inputType"": {
                                        ""type"": ""Choice"",
                                        ""inputChoices"": [
                                            {
                                                ""type"": ""ChoiceOptionStatic"",
                                                ""text"": ""Add Comment"",
                                                ""intent"": ""addcomment"",
                                                ""id"": ""0a46be03-a300-46a6-957d-f530b72536df""
                                            },
                                            {
                                                ""type"": ""ChoiceOptionStatic"",
                                                ""text"": ""View Comments"",
                                                ""intent"": ""viewcomments"",
                                                ""id"": ""4424728c-41b4-4aa7-8c5b-00d248c5f818""
                                            },
                                            {
                                                ""type"": ""ChoiceOptionStatic"",
                                                ""text"": ""Done"",
                                                ""intent"": ""done"",
                                                ""id"": ""a95eaadf-4520-4488-bb7a-791ae42b4731""
                                            }
                                        ],
                                        ""acceptUnmatchedResponse"": false,
                                        ""choices"": []
                                    },
                                    ""variableAssignment"": {
                                        ""type"": ""UpdateVariable"",
                                        ""variableRef"": ""$72919745-3c3b-4c4e-bdfa-e86afdd32f03$""
                                    },
                                    ""id"": ""tj-bd0f8179-47ab-4e63-bc61-081b9737b73a"",
                                    ""title"": """"
                                },
                                {
                                    ""type"": ""Switch"",
                                    ""conditionVariableRef"": ""$72919745-3c3b-4c4e-bdfa-e86afdd32f03$"",
                                    ""cases"": [
                                        {
                                            ""id"": ""136b1e06-76f8-40e6-a031-f0834a9c67db"",
                                            ""value"": ""viewcomments"",
                                            ""actions"": [
                                                {
                                                    ""type"": ""CallProjectFlow"",
                                                    ""flowRef"": ""ServiceNow.ShowIncidentComments"",
                                                    ""arguments"": [
                                                        {
                                                            ""paramRef"": ""067b45ac-b44a-4f8e-a165-bafac66d07b8"",
                                                            ""paramVariableType"": ""Object"",
                                                            ""value"": ""=${$abf99f84-a503-4514-9b40-11e2ca91b7e9$}\n""
                                                        },
                                                        {
                                                            ""paramRef"": ""1ed9fbe4-cda9-4395-9041-7e5eb9d6e214"",
                                                            ""paramVariableType"": ""Object"",
                                                            ""value"": ""=first(where(${$a091ea6f-bc84-4f32-a08f-1f45798fd7b2$}, i => i.sysId == ${$412f41ee-80fe-4550-89d7-7ff30eb45423$}))\n""
                                                        }
                                                    ],
                                                    ""returns"": [],
                                                    ""id"": ""tj-faf121c7-bf45-4255-82fd-445d4feb4116"",
                                                    ""title"": """"
                                                }
                                            ]
                                        },
                                        {
                                            ""id"": ""3beabbb8-fa96-4376-b2b8-ce43f3884dee"",
                                            ""value"": ""addcomment"",
                                            ""actions"": [
                                                {
                                                    ""type"": ""CallProjectFlow"",
                                                    ""flowRef"": ""ServiceNow.AddComment"",
                                                    ""arguments"": [
                                                        {
                                                            ""paramRef"": ""ba85b433-40fa-4763-9a9c-fbff1fc44a3b"",
                                                            ""paramVariableType"": ""Object"",
                                                            ""value"": ""=${$abf99f84-a503-4514-9b40-11e2ca91b7e9$}\n""
                                                        },
                                                        {
                                                            ""paramRef"": ""93486419-a519-4d08-901b-f7cf0c9c8a55"",
                                                            ""paramVariableType"": ""Object"",
                                                            ""value"": ""=first(where(${$a091ea6f-bc84-4f32-a08f-1f45798fd7b2$}, i => i.sysId == ${$412f41ee-80fe-4550-89d7-7ff30eb45423$}))\n""
                                                        }
                                                    ],
                                                    ""returns"": [],
                                                    ""id"": ""tj-21be3725-6303-461d-a512-1654cabbe441"",
                                                    ""title"": """"
                                                }
                                            ]
                                        }
                                    ],
                                    ""defaultActions"": [],
                                    ""id"": ""tj-6763e272-6004-47bb-914e-d2c8172f217a"",
                                    ""title"": """"
                                }
                            ],
                            ""id"": ""tj-9c3abce2-4e84-4e48-acaf-e186f4a2d7e0"",
                            ""title"": """"
                        },
                        {
                            ""type"": ""AssignVariables"",
                            ""variableAssignments"": [
                                {
                                    ""expression"": ""= concat(${$5300177b-3560-4319-a38e-1a2fd454fe52$}, [1])"",
                                    ""assignment"": {
                                        ""type"": ""UpdateVariable"",
                                        ""variableRef"": ""$5300177b-3560-4319-a38e-1a2fd454fe52$""
                                    }
                                }
                            ],
                            ""id"": ""tj-03338505-e9b1-4644-ba32-8ce3b9e3afd9"",
                            ""title"": """"
                        }
                    ],
                    ""id"": ""tj-2bb97ddc-5ef3-4eb3-9288-ef9f39d323d9"",
                    ""title"": """"
                }
            ]
        },
        ""inputParameters"": [],
        ""returnParameters"": []
    }
}";

            var redactor = new Redactor();
            var result = redactor.Redact(json);

            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse(redactedJson);
            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true), () =>
            {
                Console.WriteLine(resultJToken.ToJsonWithNoTypeNameHandling());
                Console.WriteLine();
                Console.WriteLine(expectedJToken.ToJsonWithNoTypeNameHandling());

                return "";
            });
        }

        [Test]
        public void RedactJsonExtensionMethod_Redacts()
        {
            var result = "{\"pass\":\"password\"}".RedactJson();

            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse("{\"pass\":\"**Redacted**\"}");
            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true));
        }

        [Test]
        public void RedactHeaders_RedactsHeaders()
        {
            var headerToRedact = new HttpRequestMessage();
            headerToRedact.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "123");
            headerToRedact.Headers.TryAddWithoutValidation("api-key", "secret");
            headerToRedact.Headers.TryAddWithoutValidation("Accept", "application/json");

            var redactor = new Redactor();
            var result = redactor.Redact(headerToRedact.Headers);

            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse(@"{
  ""Authorization"": [
    ""**Redacted**""
  ],
  ""api-key"": [
    ""**Redacted**""
  ],
  ""Accept"": [
    ""application/json""
  ]
}");
            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true), () => result);
        }

        [Test]
        public void Redact_DictionaryJson()
        {
            var data = new Dictionary<string, object>
            {
                ["pass"] = "123",
                ["password"] = new[] { "123", "456" }
            };
            var json = data.ToJsonWithNoTypeNameHandling();
            var redactor = new Redactor();
            var result = redactor.Redact(json);

            //assert
            var resultJToken = JToken.Parse(result);
            var expectedJToken = JToken.Parse(@"{
  ""pass"": ""**Redacted**"",
  ""password"": [ ""**Redacted**""]
}");
            Assert.That(JToken.DeepEquals(resultJToken, expectedJToken), Is.EqualTo(true), () => result);
        }

        #endregion

        #region RedactObject

        [TestCase("")]
        [TestCase(null)]
        public void RedactObject_ReturnsInput_WhenNotSet(string input)
        {
            var redactor = new Redactor();
            var result = redactor.RedactObject(input);

            //assert
            Assert.That(result, Is.EqualTo(input));
        }

        public static IEnumerable<TestCaseDataNamed> GetObjectsForRedactObject_NoRedaction()
        {
            yield return new TestCaseDataNamed("int", 12345, 12345);
            yield return new TestCaseDataNamed("long", (long)12345, (long)12345);
            yield return new TestCaseDataNamed("uint", (uint)12345, (uint)12345);
            yield return new TestCaseDataNamed("ulong", (ulong)12345, (ulong)12345);
            yield return new TestCaseDataNamed("double", (double)12345.65, (double)12345.65);
            yield return new TestCaseDataNamed("decimal", (decimal)12345.65, (decimal)12345.65);
            yield return new TestCaseDataNamed("byte", (byte)12, (byte)12);
            yield return new TestCaseDataNamed("sbyte", (sbyte)12, (sbyte)12);
            yield return new TestCaseDataNamed("char", (char)12, (char)12);
            yield return new TestCaseDataNamed("float", (float)99, (float)99);

            yield return new TestCaseDataNamed("AnyonmousType_Object", new { name = "bob" }, new { name = "bob" });
            yield return new TestCaseDataNamed("ArrayOfInt", new[] { 1, 2, 3 }, new[] { 1, 2, 3 });
            yield return new TestCaseDataNamed("ArrayOfString", (object)new[] { "1", "2", "3" }, (object)new[] { "1", "2", "3" });
            yield return new TestCaseDataNamed("AnyonmousType_ArrayOfObject", new[] { new { test = "123" } }, new[] { new { test = "123" } });

            yield return new TestCaseDataNamed("string not json", "some bit of text", "some bit of text");
            yield return new TestCaseDataNamed("string of json", "{\"test\":\"value\"}", "{\"test\":\"value\"}");

            yield return new TestCaseDataNamed("Class", new RedactObjectTestClass1 { Prop1 = "Prop1" }, new { prop1 = "Prop1" });
            yield return new TestCaseDataNamed("Record", new RedactRecordTestClass1("Prop1"), new { prop1 = "Prop1" });
            yield return new TestCaseDataNamed("Dictionary", new Dictionary<string, object> { ["key1"] = "123" }, new Dictionary<string, object> { ["key1"] = "123" });

            yield return new TestCaseDataNamed("IEnumerable<KeyValuePair<string, string>>", new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("prop1", "value1") }, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("prop1", "value1") });
            yield return new TestCaseDataNamed("IEnumerable<KeyValuePair<string, IEnumerable<string>>>", new List<KeyValuePair<string, List<string>>> { new KeyValuePair<string, List<string>>("prop1", new List<string> { "value1" }) }, new List<KeyValuePair<string, List<string>>> { new KeyValuePair<string, List<string>>("prop1", new List<string> { "value1" }) });
        }

        [TestCaseSource(nameof(GetObjectsForRedactObject_NoRedaction))]
        public void RedactObject_ReturnsInputValueWhenNoRedaction_For(object value, object expected)
        {
            var redactor = new Redactor();
            var result = redactor.RedactObject(value);

            //assert
            Assert.That(JToken.DeepEquals(JToken.FromObject(result!), JToken.FromObject(expected, JsonSerializer.Create(JsonSerializerSettingsNone))), Is.EqualTo(true), () =>
            {
                Console.WriteLine(JToken.FromObject(value).ToString(Formatting.None));
                Console.WriteLine();
                Console.WriteLine(JToken.FromObject(result!).ToString(Formatting.None));
                Console.WriteLine();
                Console.WriteLine(JToken.FromObject(expected).ToString(Formatting.None));
                return "";
            });
        }

        public static IEnumerable<TestCaseDataNamed> GetObjectsForRedactObject_Redaction()
        {
            yield return new TestCaseDataNamed("AnyonmousType_Object", new { password = "bob" }, new { password = "**Redacted**" });
            yield return new TestCaseDataNamed("AnyonmousType_Object with property as object", new { password = new { prop1 = "bob" } }, new { password = "**Redacted**" });
            yield return new TestCaseDataNamed("AnyonmousType_ArrayOfObject", new[] { new { password = "123" } }, new[] { new { password = "**Redacted**" } });
            yield return new TestCaseDataNamed("AnyonmousType_Object with property as number", new { password = 123 }, new { password = "**Redacted**" });

            yield return new TestCaseDataNamed("string of json", "{\"password\":\"value\"}", "{\"password\":\"**Redacted**\"}");

            yield return new TestCaseDataNamed("Class", new RedactObjectTestClass2 { Password = "password" }, new RedactObjectTestClass2 { Password = "**Redacted**" });
            yield return new TestCaseDataNamed("Record", new RedactRecordTestClass2("Prop1"), new RedactRecordTestClass2("**Redacted**"));
            yield return new TestCaseDataNamed("Dictionary", new Dictionary<string, object> { ["password"] = "123" }, new Dictionary<string, object> { ["password"] = "**Redacted**" });

            yield return new TestCaseDataNamed("Class property to object", new RedactObjectTestClass3 { Password = new RedactObjectTestClass4 { data = "hi" } }, new { Password = "**Redacted**" });


            yield return new TestCaseDataNamed("IEnumerable<KeyValuePair<string, string>> request headers", new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("password", "value1") }, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("password", "**Redacted**") });
            yield return new TestCaseDataNamed("IEnumerable<KeyValuePair<string, IEnumerable<string>>> response headers", new List<KeyValuePair<string, List<string>>> { new KeyValuePair<string, List<string>>("password", new List<string> { "value1" }) }, new List<KeyValuePair<string, List<string>>> { new KeyValuePair<string, List<string>>("password", new List<string> { "**Redacted**" }) });

            yield return new TestCaseDataNamed("Dictionary<string, List<string>>", new Dictionary<string, List<string>> { ["password"] = new List<string> { "value" } }, new { password = new[] { "**Redacted**" } });
            yield return new TestCaseDataNamed("Dictionary<string, object> Debug event data 1", new Dictionary<string, object>
            {
                ["httpRequest"] = new
                {
                    headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("authorization", "value") },
                    body = "{\"password\":\"value1\"}"
                }
            },
            new Dictionary<string, object>
            {
                ["httpRequest"] = new
                {
                    headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("authorization", "**Redacted**") },
                    body = "{\"password\":\"**Redacted**\"}"
                }
            });

            yield return new TestCaseDataNamed("Dictionary<string, object> Debug event data body in array", new Dictionary<string, object>
            {
                ["httpRequest"] = new
                {
                    headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("authorization", "value") },
                    body = "[{\"password\":\"value1\"}]"
                }
            },
            new Dictionary<string, object>
            {
                ["httpRequest"] = new
                {
                    headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("authorization", "**Redacted**") },
                    body = "[{\"password\":\"**Redacted**\"}]"
                }
            });

            yield return new TestCaseDataNamed("Dictionary<string, object> Debug event data body not json", new Dictionary<string, object>
            {
                ["httpRequest"] = new
                {
                    headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("authorization", "value") },
                    body = "this is not json super secret here"
                }
            },
            new Dictionary<string, object>
            {
                ["httpRequest"] = new
                {
                    headers = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("authorization", "**Redacted**") },
                    body = "this is not json super secret here"
                }
            });

            yield return new TestCaseDataNamed("Dictionary<string, object> Debug event with other object", new Dictionary<string, object>
            {
                ["object"] = new
                {
                    password = "123",
                    obj1 = new RedactObjectTestClass2 { Password = "secret" }
                }
            },
           new Dictionary<string, object>
           {
               ["object"] = new
               {
                   password = "**Redacted**",
                   obj1 = new RedactObjectTestClass2 { Password = "**Redacted**" }
               }
           });
        }

        [TestCaseSource(nameof(GetObjectsForRedactObject_Redaction))]
        public void RedactObject_ReturnsRedactedValueWhen_For(object value, object expected)
        {
            var redactor = new Redactor();
            var result = redactor.RedactObject(value);

            //assert
            Assert.That(JToken.DeepEquals(JToken.FromObject(result!), JToken.FromObject(expected, JsonSerializer.Create(JsonSerializerSettingsNone))), Is.EqualTo(true), () =>
            {
                Console.WriteLine(JToken.FromObject(value).ToString(Formatting.None));
                Console.WriteLine();
                Console.WriteLine(JToken.FromObject(result!).ToString(Formatting.None));
                Console.WriteLine();
                Console.WriteLine(JToken.FromObject(expected).ToString(Formatting.None));

                return "".ToJson();
            });
        }

        public static IEnumerable<TestCaseDataNamed> GetObjectsForRedactObject_AllowList()
        {
            yield return new TestCaseDataNamed("AnyonmousType_Object", new { TotalTokens = "bob" }, new { TotalTokens = "bob" });
            yield return new TestCaseDataNamed("AnyonmousType_Object", new { total_tokens = "bob" }, new { total_tokens = "bob" });
            yield return new TestCaseDataNamed("AnyonmousType_Object", new { completion_tokens = "bob" }, new { completion_tokens = "bob" });
            yield return new TestCaseDataNamed("AnyonmousType_Object", new { prompt_tokens = "bob" }, new { prompt_tokens = "bob" });
        }

        [TestCaseSource(nameof(GetObjectsForRedactObject_AllowList))]
        public void RedactObject_DoesNotRedact_WhenPropertiesInAllowList(object value, object expected)
        {
            var redactor = new Redactor();
            var result = redactor.RedactObject(value);

            //assert
            Assert.That(JToken.DeepEquals(JToken.FromObject(result!), JToken.FromObject(expected, JsonSerializer.Create(JsonSerializerSettingsNone))), Is.EqualTo(true), () =>
            {
                Console.WriteLine(JToken.FromObject(value).ToString(Formatting.None));
                Console.WriteLine();
                Console.WriteLine(JToken.FromObject(result!).ToString(Formatting.None));
                Console.WriteLine();
                Console.WriteLine(JToken.FromObject(expected).ToString(Formatting.None));

                return "".ToJson();
            });
        }

        [Test]
        public void RedactObject_DateTimeOffsetBackToDateTimeOffset()
        {
            var data = new { timestamp = new DateTimeOffset(2023, 08, 07, 17, 0, 0, TimeSpan.Zero) };
            var redactor = new Redactor();
            var result = redactor.RedactObject(data);

            //assert
            var resultJson = result!.ToJsonWithNoTypeNameHandling();

            Console.WriteLine($"{resultJson}");

            var jObject = resultJson.FromJson<JObject>();

            JToken jValue = jObject!["timestamp"]!;

            var timestamp = jValue.Value<DateTimeOffset>();

            Assert.That(timestamp, Is.EqualTo(new DateTimeOffset(2023, 08, 07, 17, 0, 0, TimeSpan.Zero)));
        }

        [Test]
        public void RedactObject_DateTimeToDateTimeOffset()
        {
            var data = new { timestamp = new DateTime(2023, 08, 07, 17, 0, 0).EnsureUtc() };
            var redactor = new Redactor();
            var result = redactor.RedactObject(data);

            //assert
            var resultJson = result!.ToJsonWithNoTypeNameHandling();
            var jObject = resultJson.FromJson<JObject>();

            var timestamp = jObject!["timestamp"]!.Value<DateTimeOffset>();

            Assert.That(timestamp, Is.EqualTo(new DateTimeOffset(2023, 08, 07, 17, 0, 0, TimeSpan.Zero)));
        }

        [Test]
        public void RedactObject_EnumToString()
        {
            var data = new { enumValue = TestEnum.Begin };
            var redactor = new Redactor();
            dynamic? result = redactor.RedactObject(data);

            //assert
            Assert.That((string)result!.enumValue, Is.EqualTo("Begin"));
        }


        private enum TestEnum
        {
            Begin = 1
        }

        private class RedactObjectTestClass1
        {
            public string Prop1 { get; set; } = string.Empty;
        }
        private class RedactObjectTestClass2
        {
            public string Password { get; set; } = string.Empty;
        }

        private class RedactObjectTestClass3
        {
            public RedactObjectTestClass4? Password { get; set; }
        }

        private class RedactObjectTestClass4
        {
            public string data { get; set; } = string.Empty;
        }


        private record RedactRecordTestClass1(string Prop1);

        private record RedactRecordTestClass2(string Password);

        #endregion
    }
}
