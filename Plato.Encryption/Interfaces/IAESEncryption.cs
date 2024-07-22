namespace Plato.Encryption.Interfaces
{
    public interface IAESEncryption
    {
        public Task<string> Encrypt(string text);
        public Task<string> Decrypt(string encryptedText);
    }
}
