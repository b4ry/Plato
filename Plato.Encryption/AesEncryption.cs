using System.Security.Cryptography;

namespace Plato.Encryption
{
    public class AesEncryption
    {
        private readonly Aes _aes;

        public AesEncryption()
        {
            _aes = Aes.Create();

            if (!File.Exists(Path.Combine(Constants.AesInitialVectorFileName)))
            {
                SaveToFile(Constants.AesInitialVectorFileName, _aes.IV, _aes.IV.Length);
            }
            else
            {
                _aes.IV = ReadFromFile(Constants.AesInitialVectorFileName, _aes.IV.Length);
            }

            if (!File.Exists(Path.Combine(Constants.AesEncryptionKeyFileName)))
            {
                SaveToFile(Constants.AesEncryptionKeyFileName, _aes.Key, _aes.Key.Length);
            }
            else
            {
                _aes.Key = ReadFromFile(Constants.AesEncryptionKeyFileName, _aes.Key.Length);
            }
        }

        private void SaveToFile(string fileName, byte[] bytes, int bytesToWrite)
        {
            FileStream fileStream = new(Path.Combine(fileName), FileMode.OpenOrCreate);

            fileStream.Write(bytes, 0, bytesToWrite);
        }

        private static byte[] ReadFromFile(string fileName, int bytesToRead)
        {
            byte[] bytes = new byte[bytesToRead];

            int numBytesRead = 0;

            while (bytesToRead > 0)
            {
                using FileStream fileStream = new(Path.Combine(fileName), FileMode.Open);

                int n = fileStream.Read(bytes, numBytesRead, bytesToRead);
                if (n == 0) break;

                numBytesRead += n;
                bytesToRead -= n;
            }

            return bytes;
        }
    }
}
