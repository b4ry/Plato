namespace Plato.Encryption
{
    public interface IAesEncryptor
    {
        Task<string> Encrypt(string text);
        Task<string> Decrypt(string encryptedText);
    }
}
