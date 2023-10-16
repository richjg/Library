using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class CodeGeneratorTests
    {
        [Test]
        public void GenerateCode_GeneratesACodeThatMeetsCodeSpecification()
        {
            var codeGenerator = new CodeGenerator();
            for (int i = 0; i < 10; i++)
            {
                var result = codeGenerator.GenerateCode(20);
                Assert.That(result.All(c => char.IsDigit(c)), Is.True);
            }
        }

    }
}
