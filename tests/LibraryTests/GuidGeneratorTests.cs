using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class GuidGeneratorTests
    {
        [Test]
        public void GenerateGuid_GeneratesANewGuidEachTime()
        {
            //Arrange
            var guidGenerator = new GuidGenerator();

            //Act
            List<Guid> guids = new List<Guid>();
            for (int i = 0; i < 1000; i++)
            {
                guids.Add(guidGenerator.GenerateGuid());
            }

            //Assert
            Assert.That(guids.Count, Is.EqualTo(1000));
            Assert.That(guids.Distinct().Count(), Is.EqualTo(1000));
            Assert.That(guids.All(g => Guid.TryParse(g.ToString(), out var _)), Is.True);
        }
    }
}
