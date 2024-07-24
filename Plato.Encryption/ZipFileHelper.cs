using Ionic.Zip;
using System.Security.Cryptography;

namespace Plato.Encryption
{
    internal static class ZipFileHelper
    {
        internal async static Task<(byte[] iv, byte[] key)> GetAesParameters(Aes aes, string password)
        {
            var iv = aes.IV;
            var key = aes.Key;

            if (File.Exists(Constants.Constants.AesAssetsZipFolderName))
            {
                using var zip = ZipFile.Read(Constants.Constants.AesAssetsZipFolderName);
                zip.Password = password;

                foreach (ZipEntry entry in zip)
                {
                    if (entry.FileName == Constants.Constants.AesInitialVectorFileName)
                    {
                        await ReadFromFile(password, iv, entry);
                    }
                    else if (entry.FileName == Constants.Constants.AesEncryptionKeyFileName)
                    {
                        await ReadFromFile(password, key, entry);
                    }
                }
            }
            else
            {
                using var zip = new ZipFile();
                zip.Password = password;

                SaveToFile(Constants.Constants.AesInitialVectorFileName, iv, zip);
                SaveToFile(Constants.Constants.AesEncryptionKeyFileName, key, zip);
            }

            return (iv, key);
        }

        private static async Task<byte[]> ReadFromFile(string password, byte[] bytes, ZipEntry entry)
        {
            MemoryStream ms = new();

            entry.ExtractWithPassword(ms, password);
            ms.Position = 0;

            await ms.ReadAsync(bytes);

            return bytes;
        }

        private static void SaveToFile(string fileName, byte[] bytes, ZipFile zip)
        {
            using MemoryStream memoryStream = new(bytes);

            zip.AddEntry(fileName, memoryStream);
            zip.Save(Constants.Constants.AesAssetsZipFolderName);
        }
    }
}
