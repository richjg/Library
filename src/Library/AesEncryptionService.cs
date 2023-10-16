using System.Security.Cryptography;

namespace Library
{
    public class AesEncryptionService : IEncryptionService
    {
        private const int saltSize = 32;
        private const int iterations = 10;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public byte[] Encrypt(string password, string data)
        {
            using var keyDerivationFunction = new Rfc2898DeriveBytes(password, saltSize, iterations, HashAlgorithm);

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 256;
            var saltBytes = keyDerivationFunction.Salt;
            var keyBytes = keyDerivationFunction.GetBytes(aes.KeySize / 8);
            var ivBytes = keyDerivationFunction.GetBytes(aes.BlockSize / 8);

            using var encryptor = aes.CreateEncryptor(keyBytes, ivBytes);
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                memoryStream.Write(saltBytes, 0, saltBytes.Length);
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(data);
                }
            }

            return memoryStream.ToArray();
        }

        public string Decrypt(string password, byte[] data)
        {
            var saltBytes = data[..saltSize];
            var ciphertextBytes = data[saltSize..];

            using var keyDerivationFunction = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithm);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            var keyBytes = keyDerivationFunction.GetBytes(aes.KeySize / 8);
            var ivBytes = keyDerivationFunction.GetBytes(aes.BlockSize / 8);

            using var decryptor = aes.CreateDecryptor(keyBytes, ivBytes);
            using var memoryStream = new MemoryStream(ciphertextBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
    }
}
