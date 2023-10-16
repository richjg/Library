using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class KeyGeneratorTests
    {
        [Test]
        public void Generate_GeneratesAKey()
        {
            var keyGeneraror = new KeyGenerator();

            //act
            var result = keyGeneraror.Generate();

            //assert
            Assert.That(result.Length, Is.EqualTo(64));
            Assert.That(result, Has.Some.Not.EqualTo(0));
        }

        [Test]
        public void Generate_GeneratesAKey_WhenLengthSet()
        {
            var keyGeneraror = new KeyGenerator();

            //act
            var result = keyGeneraror.Generate(1);

            //assert
            Assert.That(result.Length, Is.EqualTo(1));
        }
    }
}
