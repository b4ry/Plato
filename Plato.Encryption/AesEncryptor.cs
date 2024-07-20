using System.Security.Cryptography;
using System.Text;

namespace Plato.Encryption
{
    public class AesEncryptor : IAesEncryptor, IDisposable
    {
        private readonly Aes _aes;
        private readonly ICryptoTransform _aesEncryptor;
        private readonly ICryptoTransform _aesDecryptor;

        public AesEncryptor()
        {
            _aes = Aes.Create();

            if (!File.Exists(Path.Combine(Constants.Constants.AesInitialVectorFileName)))
            {
                SaveToFile(Constants.Constants.AesInitialVectorFileName, _aes.IV, _aes.IV.Length);
            }
            else
            {
                _aes.IV = ReadFromFile(Constants.Constants.AesInitialVectorFileName, _aes.IV.Length);
            }

            if (!File.Exists(Path.Combine(Constants.Constants.AesEncryptionKeyFileName)))
            {
                SaveToFile(Constants.Constants.AesEncryptionKeyFileName, _aes.Key, _aes.Key.Length);
            }
            else
            {
                _aes.Key = ReadFromFile(Constants.Constants.AesEncryptionKeyFileName, _aes.Key.Length);
            }

            _aesEncryptor = _aes.CreateEncryptor();
            _aesDecryptor = _aes.CreateDecryptor();
        }

        public async Task<string> Encrypt(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, _aesEncryptor, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(textBytes);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public async Task<string> Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, _aesDecryptor, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(encryptedBytes);
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private void SaveToFile(string fileName, byte[] bytes, int bytesToWrite)
        {
            FileStream fileStream = new(Path.Combine(fileName), FileMode.OpenOrCreate);

            fileStream.WriteAsync(bytes, 0, bytesToWrite);
        }

        private static byte[] ReadFromFile(string fileName, int bytesToRead)
        {
            byte[] bytes = new byte[bytesToRead];

            int numBytesRead = 0;

            while (bytesToRead > 0)
            {
                using FileStream fileStream = new(Path.Combine(fileName), FileMode.Open);

                int n = fileStream.Read(bytes, numBytesRead, bytesToRead);

                if (n == 0)
                {
                    break;
                }

                numBytesRead += n;
                bytesToRead -= n;
            }

            return bytes;
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
                    _aesEncryptor.Dispose();
                    _aesDecryptor.Dispose();
                    _aes.Dispose();
                }
            }
        }
    }
}
