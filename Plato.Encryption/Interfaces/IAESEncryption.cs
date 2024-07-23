namespace Plato.Encryption.Interfaces
{
    public interface IAESEncryption
    {
        public byte[] Key { get; }
        public byte[] IV { get; }

        public Task<string> Encrypt(string text);
        public Task<string> Decrypt(string encryptedText);
    }
}
