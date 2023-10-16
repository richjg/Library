using Library;
using NUnit.Framework;

namespace LibraryTests
{
    [TestFixture]
    public class ExceptionExtensionTests
    {
        [Test]
        public void FormatAsJson_FormatsAException_AsJson()
        {
            //Arrange
            var exception = new Exception("TestMessage");

            //Act
            var result = exception.FormatAsJson();

            var data = result.FromJson<List<JsonExceptionFormat>>();

            Assert.That(data!.Count, Is.EqualTo(1));
            Assert.That(data![0].Type, Is.EqualTo("Exception"));
            Assert.That(data![0].Message, Is.EqualTo("TestMessage"));
        }

        [Test]
        public void FormatAsJson_FormatsExceptionAndInnerExceptions_AsJson()
        {
            //Arrange
            var exception = new Exception("Exception1", new Exception("Exception2", new Exception("Exception3")));

            //Act
            var result = exception.FormatAsJson();

            var data = result.FromJson<List<JsonExceptionFormat>>();

            Assert.That(data!.Count, Is.EqualTo(3));
            Assert.That(data![0].Message, Is.EqualTo("Exception1"));
            Assert.That(data![1].Message, Is.EqualTo("Exception2"));
            Assert.That(data![2].Message, Is.EqualTo("Exception3"));
        }

        [Test]
        public void FormatAsJson_FormatsAggreateExceptionAndInnerExceptions_AsJson()
        {
            //Arrange
            var exception = new AggregateException("Exception1", new Exception("Exception2"), new Exception("Exception3", new Exception("Exception4")));

            //Act
            var result = exception.FormatAsJson();

            var data = result.FromJson<List<JsonExceptionFormat>>();

            Assert.That(data!.Count, Is.EqualTo(4));
            Assert.That(data![0].Message, Is.EqualTo("Exception1 (Exception2) (Exception3)"));
            Assert.That(data![1].Message, Is.EqualTo("Exception2"));
            Assert.That(data![2].Message, Is.EqualTo("Exception3"));
            Assert.That(data![3].Message, Is.EqualTo("Exception4"));
        }

        [Test]
        public void FormatAsJson_FormatsExceptionWithStackTrace_AsJson()
        {
            //Arrange
            Exception? exception = null;
            try
            {
                var a = 0;
                var b = 5 / a;
            }
            catch (Exception e)
            {
                exception = e;
            }

            //Act
            var result = exception!.FormatAsJson();

            var data = result.FromJson<List<JsonExceptionFormat>>();

            Assert.That(data![0].StackTrace, Contains.Substring("ExceptionExtensionTests.FormatAsJson_FormatsExceptionWithStackTrace"));
        }

        [Test]
        public void FormatAsJson_FormatsExceptionWithDataItems_AsJson()
        {
            //Arrange
            var exception = new Exception("Exception1").AddData("Item1", "Value1").AddData("Item2", "Value2");

            //Act
            var result = exception.FormatAsJson();

            var data = result.FromJson<List<JsonExceptionFormat>>()!;
            Assert.That(data[0].Data.Count, Is.EqualTo(2));
            Assert.That(data[0].Data["Item1"], Is.EqualTo("Value1"));
            Assert.That(data[0].Data["Item2"], Is.EqualTo("Value2"));
        }

        [Test]
        public void FormatAsJson_FormatsExceptionWithDataItemsAsJson_WhenValueIsNotAString()
        {
            //Arrange
            var exception = new Exception("Exception1").AddData("Item1", new { Name = "test" });

            //Act
            var result = exception.FormatAsJson();

            Assert.That(result, Is.EqualTo(@"[{""type"":""Exception"",""message"":""Exception1"",""stackTrace"":"""",""data"":{""Item1"":{""name"":""test""}}}]"));
        }

        [Test]
        public void FormatAsJson_FormatsExceptionIgnoresDataItemsWhereKeyIsNotStringType_AsJson()
        {
            //Arrange
            var exception = new Exception("Exception1");
            exception.Data.Add(23, "test");

            //Act
            var result = exception.FormatAsJson();

            var data = result.FromJson<List<JsonExceptionFormat>>();
            Assert.That(data![0].Data.Count, Is.EqualTo(0));
        }
    }

}
