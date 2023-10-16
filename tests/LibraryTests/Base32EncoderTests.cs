using System.Text;
using Library;
using NUnit.Framework;

namespace LibraryTests
{
    [TestFixture]
    public class Base32EncoderTests
    {
        [TestCase("A", "IE======")]
        [TestCase("AB", "IFBA====")]
        [TestCase("ABC", "IFBEG===")]
        [TestCase("ABCD", "IFBEGRA=")]
        [TestCase("ABCDE", "IFBEGRCF")]
        public void ToBase32String_EncodesAsciiData(string asciiInput, string expectedOutput)
        {
            //Arrange
            var bytes = Encoding.ASCII.GetBytes(asciiInput);

            //Act
            var result = Base32Encoder.ToBase32String(bytes);

            //Assert
            Assert.That(result, Is.EqualTo(expectedOutput));
        }

        [TestCase("IE======", "A")]
        [TestCase("IFBA====", "AB")]
        [TestCase("IFBEG===", "ABC")]
        [TestCase("IFBEGRA=", "ABCD")]
        [TestCase("IFBEGRCF", "ABCDE")]
        public void FromBase32String_DecodesAsciiData(string input, string expectedAsciiOutput)
        {
            //Arrange
            //Act
            var result = Encoding.ASCII.GetString(Base32Encoder.FromBase32String(input) ?? Array.Empty<byte>());

            //Assert
            Assert.That(result, Is.EqualTo(expectedAsciiOutput));
        }

        [Test]
        public void ToBase32String_EncodesGuid()
        {
            //Arrange
            var bytes = new Guid("{3f2d3263-1366-438a-9664-d8ed3e006d25}").ToByteArray();

            //Act
            var result = Base32Encoder.ToBase32String(bytes);

            //Assert
            Assert.That(result, Is.EqualTo("MMZC2P3GCOFEHFTE3DWT4ADNEU======"));
        }

        [Test]
        public void FromBase32String_DecodesGuid()
        {
            //Arrange
            //Act
            var result = new Guid(Base32Encoder.FromBase32String("MMZC2P3GCOFEHFTE3DWT4ADNEU======")!);

            //Assert
            Assert.That(result, Is.EqualTo(new Guid("{3f2d3263-1366-438a-9664-d8ed3e006d25}")));
        }
    }
}