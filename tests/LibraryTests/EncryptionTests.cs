using Library;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryTests
{
    public class EncryptionTests
    {
        [Test]
        public void EncryptDecrypt_RoundTrips()
        {
            var service = new AesEncryptionService();

            var encrypted = service.Encrypt("password1", "Hello world");
            var decrypted = service.Decrypt("password1", encrypted);

            Assert.That(decrypted, Is.EqualTo("Hello world"));
        }

        [Test]
        public void Encrypt_ReturnsDifferentResultForSameInput()
        {
            var service = new AesEncryptionService();

            var encrypted1 = service.Encrypt("password1", "Hello world");
            var encrypted2 = service.Encrypt("password1", "Hello world");

            //Console.WriteLine(encrypted1.ToHexString().Select((x, i) => i % 2 == 0 ? $"0x{x}" : $"{x}, ").Concat(""));
            Assert.That(encrypted1, Is.Not.EqualTo(encrypted2));
        }

        [Test]
        public void Decrypt_ReturnsDataDecrypted()
        {
            var service = new AesEncryptionService();

            var result = service.Decrypt("password1", new byte[] { 0xE6, 0x3A, 0x8C, 0x72, 0x9E, 0xD0, 0xD1, 0x48, 0x8D, 0x8E, 0x78, 0x13, 0x63, 0x88, 0xE4, 0xFA, 0x56, 0xE2, 0x10, 0x9D, 0x71, 0xE3, 0xC8, 0x71, 0x08, 0x74, 0xBF, 0xE5, 0xD9, 0xDD, 0xE7, 0x40, 0xC2, 0xD7, 0x24, 0xB3, 0x93, 0x11, 0xF0, 0x40, 0x45, 0x5D, 0x35, 0x68, 0x36, 0xB9, 0xB6, 0x42 });

            Assert.That(result, Is.EqualTo("Hello world"));
        }

        [Test]
        public void Encrypt_ReturnsEmpty_WhenInputIsEmpty()
        {
            var service = new AesEncryptionService();

            var encrypted = service.Encrypt("password1", "");

            //Console.WriteLine(encrypted.ToHexString().Select((x, i) => i % 2 == 0 ? $"0x{x}" : $"{x}, ").Concat(""));

            Assert.That(encrypted, Is.Not.Empty);
        }

        [Test]
        public void Decrypt_ReturnsEmpty_WhenEmptyIsWhatWasEncrypted()
        {
            var service = new AesEncryptionService();

            var result = service.Decrypt("password1", new byte[] { 0xA0, 0xAC, 0x1A, 0x36, 0x76, 0xBC, 0x0D, 0x33, 0x36, 0x7E, 0x7D, 0xAE, 0x73, 0x61, 0x5D, 0xE4, 0x44, 0x34, 0xD1, 0xD8, 0xC3, 0x96, 0x51, 0x23, 0xDE, 0xEE, 0x89, 0x31, 0x50, 0xE0, 0x06, 0x1A, 0xE8, 0x07, 0x10, 0x20, 0x37, 0xB8, 0x4B, 0x32, 0xD9, 0x6D, 0x9F, 0xC4, 0xE9, 0x46, 0x59, 0x58, });

            Assert.That(result, Is.EqualTo(""));
        }

    }
}
