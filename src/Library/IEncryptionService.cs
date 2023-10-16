namespace Library
{
    public interface IEncryptionService
    {
        byte[] Encrypt(string password, string data);
        string Decrypt(string password, byte[] data);
    }
}
