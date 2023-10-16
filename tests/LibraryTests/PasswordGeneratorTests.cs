using System.Security.Cryptography;
using System.Text;
using Library;
using NUnit.Framework;

namespace LibraryTests
{
    public class PasswordGeneratorTests
    {
        [Test]
        public void GeneratePassword_GeneratesAPassword_ThatMeetsThePassowrdSpecification()
        {
            var passwordGenerator = new PasswordGenerator();
            for (int i = 0; i < 1000; i++)
            {
                var result = passwordGenerator.GeneratePassword(20);
                Assert.That(result.Any(c => char.IsUpper(c)), Is.True);
                Assert.That(result.Any(c => char.IsLower(c)), Is.True);
                Assert.That(result.Any(c => char.IsDigit(c)), Is.True);
                Assert.That(result.Any(c => char.IsUpper(c) == false && char.IsLower(c) == false && char.IsDigit(c) == false), Is.True);
            }
        }

        [Test]
        public void GeneratePassword_GeneratesAPassword_ThatDoesntNeedUrlEncoding_CosItBreaksServiceNowSkill()
        {
            var passwordGenerator = new PasswordGenerator();
            for (int i = 0; i < 1000; i++)
            {
                var result = passwordGenerator.GeneratePassword(20);
                var resultUrlEncoded = Uri.EscapeDataString(result);
                Assert.That(resultUrlEncoded, Is.EqualTo(result));
            }
        }

        [Test]
        public void GeneratePasswordWithHash_GeneratesAPasswordWithTheHash()
        {
            var passwordGenerator = new PasswordGenerator();
            //act
            var (plainTextPassword, hash) = passwordGenerator.GeneratePasswordWithHash();

            //Assert
            using var sha256 = SHA256.Create();
            var sha256B64Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(plainTextPassword)));

            Assert.That(plainTextPassword, Is.Not.Empty);
            Assert.That(hash, Is.EqualTo(sha256B64Hash));
        }
    }
}
