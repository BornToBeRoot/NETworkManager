using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace NETworkManager.Models.Cryptographie
{
    public static class FileCryptoHelper
    {
        private const int KeySize = 256;
        private const int Iterations = 100000;

        public static byte[] Encrypt(byte[] text, string password)
        {
            var salt = Generate256BitsOfRandomEntropy();
            var iv = Generate256BitsOfRandomEntropy();

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                var key = rfc2898DeriveBytes.GetBytes(KeySize / 8); // 256 Bits / 8 Bits = 32 Bytes

                using (var rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.BlockSize = 256;
                    rijndaelManaged.Mode = CipherMode.CBC;
                    rijndaelManaged.Padding = PaddingMode.PKCS7;

                    using (var encryptor = rijndaelManaged.CreateEncryptor(key, iv))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(text, 0, text.Length);
                                cryptoStream.FlushFinalBlock();

                                var cipher = salt;
                                cipher = cipher.Concat(iv).ToArray();
                                cipher = cipher.Concat(memoryStream.ToArray()).ToArray();

                                memoryStream.Close();
                                cryptoStream.Close();

                                return cipher;
                            }
                        }
                    }
                }
            }
        }

        public static byte[] Decrypt(byte[] cipherWithSaltAndIv, string password)
        {
            var salt = cipherWithSaltAndIv.Take(KeySize / 8).ToArray(); // 256 bits / 8 bits = 32 bytes
            var iv = cipherWithSaltAndIv.Skip(KeySize / 8).Take(KeySize / 8).ToArray(); // Skip 32 bytes, take 32 Bytes iv
            var cipher = cipherWithSaltAndIv.Skip((KeySize / 8) * 2).Take(cipherWithSaltAndIv.Length - ((KeySize / 8) * 2)).ToArray(); // Skip 64 bytes, take cipher bytes (length - 64)

            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                var key = rfc2898DeriveBytes.GetBytes(KeySize / 8); // 256 Bits / 8 Bits = 32 Bytes

                using (var rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.BlockSize = 256;
                    rijndaelManaged.Mode = CipherMode.CBC;
                    rijndaelManaged.Padding = PaddingMode.PKCS7;

                    using (var decryptor = rijndaelManaged.CreateDecryptor(key, iv))
                    {
                        using (var memoryStream = new MemoryStream(cipher))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var text = new byte[cipher.Length];
                                cryptoStream.Read(text, 0, text.Length);

                                memoryStream.Close();
                                cryptoStream.Close();

                                return text;
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 * 8 = 256 Bits.

            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(randomBytes);
            }

            return randomBytes;
        }
    }
}
