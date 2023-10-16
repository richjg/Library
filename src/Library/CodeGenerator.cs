using System.Security.Cryptography;

namespace Library
{
    public interface ICodeGenerator
    {
        string GenerateCode(int length = 6);
    }

    public class CodeGenerator : ICodeGenerator
    {
        private readonly char[] characters = $"0123456789".ToCharArray();

        public string GenerateCode(int length = 6)
        {
            if (length <= 4)
            {
                throw new ArgumentException("length must be greater than 4", nameof(length));
            }

            var buf = new byte[length];
            string code = "";
            using (var rnd = RandomNumberGenerator.Create())
            {
                rnd.GetBytes(buf);

                code = string.Create(length, (buf, characters, length), (dst, state) =>
                {
                    for (int iter = 0; iter < state.length; iter++)
                    {
                        int i = buf[iter] % state.characters.Length;
                        dst[iter] = state.characters[i];
                    }
                });
            }
            return code;
        }
    }
}
