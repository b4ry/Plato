namespace Plato.Encryption.Interfaces
{
    public interface IRSAEncryption
    {
        public byte[] Encrypt(byte[] messageBytes);
        public void BuildFromXmlString(string asymmetricPublicKey);
    }
}
