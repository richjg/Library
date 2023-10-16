using NUnit.Framework;

namespace LibraryTests
{
    public class EnumParseTest
    {
        public enum TestEnum
        {
            Value1,
            Value2
        }

        [Test]
        public void ToEnum_ReturnsConvertsStringToEnumValue()
        {
            var result = "Value1".ToEnum<TestEnum>();
            Assert.That(result, Is.EqualTo(TestEnum.Value1));
        }

        [Test]
        public void ToEnum_IgnoreCasing_WhenConverting()
        {
            var result = "vAlUe1".ToEnum<TestEnum>();
            Assert.That(result, Is.EqualTo(TestEnum.Value1));
        }

        [Test]
        public void ToEnum_Trims_InputStringBeforeConverting()
        {
            var result = "   Value1    ".ToEnum<TestEnum>();
            Assert.That(result, Is.EqualTo(TestEnum.Value1));
        }

        [Test]
        public void ToEnum_Throws_WhenValueCantBeFound()
        {
            void Act()
            {
                "NotFound".ToEnum<TestEnum>();
            }

            Assert.That(Act, Throws.Exception.With.Message.Contains("Requested value 'NotFound' was not found."));
        }

        [Test]
        public void ToEnum_ReturnsDefaultValue_WhenTheEnumValueIsNotFound()
        {
            var result = "NotFound".ToEnum<TestEnum>(TestEnum.Value2);
            Assert.That(result, Is.EqualTo(TestEnum.Value2));
        }

        [Test]
        public void ToEnum_RetutnsConvertedStringNotDefaultValue_WhenEnumValueIsFound()
        {
            var result = "Value2".ToEnum<TestEnum>(TestEnum.Value2);
            Assert.That(result, Is.EqualTo(TestEnum.Value2));
        }

    }
}
