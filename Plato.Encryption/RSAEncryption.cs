using Plato.Encryption.Interfaces;
using System.Security.Cryptography;

namespace Plato.Encryption
{
    public class RSAEncryption : IRSAEncryption
    {
        private readonly RSA _rsa;

        public RSAEncryption()
        {
            _rsa = RSA.Create();
        }

        public void FromXmlString(string asymmetricPublicKey)
        {
            _rsa.FromXmlString(asymmetricPublicKey);
        }

        public byte[] Encrypt(byte[] messageBytes)
        {
            return _rsa.Encrypt(messageBytes, RSAEncryptionPadding.Pkcs1);
        }
    }
}
