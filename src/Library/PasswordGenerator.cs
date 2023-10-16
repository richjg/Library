using System.Security.Cryptography;
using System.Text;

namespace Library
{
    public static class IPasswordGeneratorExtensions
    {
        public static (string plainText, string sha256B64Hash) GeneratePasswordWithHash(this IPasswordGenerator passwordGenerator, int length = 20)
        {
            using var sha256 = SHA256.Create();
            var plainText = passwordGenerator.GeneratePassword(length);
            var sha256B64Hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(plainText)));
            return (plainText, sha256B64Hash);
        }
    }

    public interface IPasswordGenerator
    {
        string GeneratePassword(int length = 20);
    }

    public class PasswordGenerator : IPasswordGenerator
    {
        private const string punctuations = "-_.~";
        private static readonly char[] characters = $"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefhijklmnopqrstuvwxyz{punctuations}".ToCharArray();

        public string GeneratePassword(int length = 20)
        {
            if (length <= 4)
            {
                throw new ArgumentException("length must be greater than 4", nameof(length));
            }

            var buf = new byte[length];
            string password = "";
            using (var rnd = RandomNumberGenerator.Create())
            {
                do
                {
                    rnd.GetBytes(buf);
                    password = string.Create(length, (buf, characters, length), (dst, state) =>
                    {
                        for (int iter = 0; iter < state.length; iter++)
                        {
                            int i = buf[iter] % state.characters.Length;
                            dst[iter] = state.characters[i];
                        }
                    });
                }
                while (ContainsUpperCase(password) == false || ContainsLowerCase(password) == false || ContainsDigit(password) == false || ContainsPunctuation(password) == false);
            }
            return password;
        }

        private bool ContainsLowerCase(string password) => password.Any(c => char.IsLower(c));
        private bool ContainsUpperCase(string password) => password.Any(c => char.IsUpper(c));
        private bool ContainsDigit(string password) => password.Any(c => char.IsDigit(c));
        private bool ContainsPunctuation(string password) => password.Any(c => punctuations.Contains(c));
    }
}
