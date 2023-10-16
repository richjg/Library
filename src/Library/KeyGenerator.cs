using System.Security.Cryptography;

namespace Library
{
    public interface IKeyGenerator
    {
        byte[] Generate(int length = 64);
    }

    public class KeyGenerator : IKeyGenerator
    {
        public byte[] Generate(int length = 64)
        {
            var buf = new byte[length];
            using var rnd = RandomNumberGenerator.Create();
            rnd.GetBytes(buf);
            return buf;
        }
    }
}
