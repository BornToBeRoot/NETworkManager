using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace NETworkManager.Utilities
{
    public static class CryptoHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="decryptedBytes"></param>
        /// <param name="password"></param>
        /// <param name="keySize"></param>
        /// <param name="blockSize"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] decryptedBytes, string password, int keySize, int blockSize, int iterations)
        {
            var salt = GenerateRandomEntropy(keySize / 8); // Generate salt based
            var iv = GenerateRandomEntropy(blockSize / 8); // Generate iv, has to be the same as the block size

            using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);

            var key = rfc2898DeriveBytes.GetBytes(keySize / 8);

            using var rijndaelManaged = new RijndaelManaged
            {
                KeySize = keySize,
                BlockSize = blockSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            using var encryptor = rijndaelManaged.CreateEncryptor(key, iv);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            cryptoStream.Write(decryptedBytes, 0, decryptedBytes.Length);
            cryptoStream.FlushFinalBlock();

            var cipher = salt;
            cipher = cipher.Concat(iv).ToArray();
            cipher = cipher.Concat(memoryStream.ToArray()).ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            return cipher;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptedBytesWithSaltAndIV"></param>
        /// <param name="password"></param>
        /// <param name="keySize"></param>
        /// <param name="blockSize"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] encryptedBytesWithSaltAndIV, string password, int keySize, int blockSize, int iterations)
        {
            var salt = encryptedBytesWithSaltAndIV.Take(keySize / 8).ToArray(); // Take salt bytes
            var iv = encryptedBytesWithSaltAndIV.Skip(keySize / 8).Take(blockSize / 8).ToArray(); // Skip salt bytes, take iv bytes
            var cipher = encryptedBytesWithSaltAndIV.Skip((keySize / 8) + (blockSize / 8)).Take(encryptedBytesWithSaltAndIV.Length - ((keySize / 8) + (blockSize / 8))).ToArray(); // Skip salt and iv bytes, take cipher bytes

            using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations);

            var key = rfc2898DeriveBytes.GetBytes(keySize / 8);

            using var rijndaelManaged = new RijndaelManaged
            {
                KeySize = keySize,
                BlockSize = blockSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            using var decryptor = rijndaelManaged.CreateDecryptor(key, iv);
            using var memoryStream = new MemoryStream(cipher);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            var text = new byte[cipher.Length];

            //cryptoStream.Read(text, 0, text.Length);
            // Fix for issue: https://github.com/dotnet/runtime/issues/61535

            int readBytes = 0;
            while (readBytes < text.Length)
            {
                int n = cryptoStream.Read(text, readBytes, text.Length - readBytes);

                if (n == 0)
                    break;

                readBytes += n;
            }

            memoryStream.Close();
            cryptoStream.Close();

            return text;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private static byte[] GenerateRandomEntropy(int size)
        {
            var randomBytes = new byte[size];

            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(randomBytes);
            }

            return randomBytes;
        }
    }
}
