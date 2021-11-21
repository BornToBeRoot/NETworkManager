using System;
using System.Security.Cryptography;

namespace NETworkManager.Utilities
{
    public static class CryptoHelper
    {
        private static readonly int blockSize = 128;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="decryptedBytes"></param>
        /// <param name="password"></param>
        /// <param name="keySize"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] decryptedBytes, string password, int keySize, int iterations)
        {
            ReadOnlySpan<byte> salt = RandomNumberGenerator.GetBytes(keySize / 8); // Generate salt based
            ReadOnlySpan<byte> iv = RandomNumberGenerator.GetBytes(blockSize / 8); // Generate iv, has to be the same as the block size

            byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA512, keySize / 8);

            using Aes aes = Aes.Create();
            aes.Key = key;

            int encryptedSize = aes.GetCiphertextLengthCbc(decryptedBytes.Length);

            byte[] cipher = new byte[salt.Length + iv.Length + encryptedSize];

            Span<byte> cipherSpan = cipher;

            salt.CopyTo(cipherSpan);
            iv.CopyTo(cipherSpan[salt.Length..]);

            int encrypted = aes.EncryptCbc(decryptedBytes, iv, cipherSpan[(salt.Length + iv.Length)..]);

            return cipher;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptedBytesWithSaltAndIV"></param>
        /// <param name="password"></param>
        /// <param name="keySize"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] encryptedBytesWithSaltAndIV, string password, int keySize, int iterations)
        {
            ReadOnlySpan<byte> salt = encryptedBytesWithSaltAndIV.AsSpan(0, keySize / 8); // Take salt bytes
            ReadOnlySpan<byte> iv = encryptedBytesWithSaltAndIV.AsSpan(keySize / 8, blockSize / 8); // Skip salt bytes, take iv bytes
            ReadOnlySpan<byte> cipher = encryptedBytesWithSaltAndIV.AsSpan((keySize / 8) + (blockSize / 8));

            byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA512, keySize / 8);

            using Aes aes = Aes.Create();
            aes.Key = key;

            return aes.DecryptCbc(cipher, iv);
        }
    }
}
