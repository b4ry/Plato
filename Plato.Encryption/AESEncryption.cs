using Plato.Encryption.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Plato.Encryption
{
    public class AESEncryption : IAESEncryption, IDisposable
    {
        private Aes? _aes;
        private ICryptoTransform? _aesEncryptor;
        private ICryptoTransform? _aesDecryptor;

        public byte[] Key { get => _aes!.Key; }
        public byte[] IV { get => _aes!.IV; }

        public async Task Create(string password)
        {
            _aes = Aes.Create();

            (_aes.IV, _aes.Key) = await ZipFileHelper.GetAesParameters(_aes, password);

            _aesEncryptor = _aes.CreateEncryptor();
            _aesDecryptor = _aes.CreateDecryptor();
        }

        public async Task<string> Encrypt(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, _aesEncryptor!, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(textBytes);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public async Task<string> Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, _aesDecryptor!, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(encryptedBytes);
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_aes != null)
                {
                    _aesEncryptor!.Dispose();
                    _aesDecryptor!.Dispose();
                    _aes.Dispose();
                }
            }
        }
    }
}
