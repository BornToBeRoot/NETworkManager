// Contains code from: https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp 
// and: https://www.vb-paradise.de/index.php/Thread/81625-verschluesseln-und-autentifizieren/#post668740

using NETworkManager.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NETworkManager.Models.Settings
{
    public static class CredentialManager
    {
        public const string CredentialsFileName = "Credentials.encrypted";

        public static ObservableCollection<CredentialInfo> Credentials { get; set; }
        public static bool CredentialsChanged { get; set; }

        private static SecureString _masterPassword;

        private static string GetCredentialsFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), CredentialsFileName);
        }

        public static void SetMasterPassword(SecureString masterPasword)
        {
            _masterPassword = masterPasword;
        }

        public static void Load(SecureString pasword)
        {            
            // Decrypt file
            if(File.Exists(GetCredentialsFilePath()))
            {

            }
            
            // if decryption was successful, save master pw for encryption
            SetMasterPassword(pasword);

            Credentials = new ObservableCollection<CredentialInfo>();
            
            // Deserialize file



            // REMOVE THIS IN RELEASE
            // Some demo content...
            AddCredential(new CredentialInfo(1, "TEST", "Admin", SecureStringHelper.ConvertToSecureString("123456")));
            AddCredential(new CredentialInfo(1, "TEST", "User1", SecureStringHelper.ConvertToSecureString("asdf")));
            AddCredential(new CredentialInfo(1, "TEST", "User2", SecureStringHelper.ConvertToSecureString("654654")));
            // - REMOVE THIS IN RELEASE


            Credentials.CollectionChanged += Credentials_CollectionChanged;
        }

        public static void Save()
        {
            // Convert CredentialInfo to CredentialInfoSerializable



            // Serialize as xml



            // Encrypt with master pw and save file




        }






        private static void Credentials_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CredentialsChanged = true;
        }

        public static void AddCredential(CredentialInfo credential)
        {
            Credentials.Add(credential);
        }

        public static void RemoveCredential(CredentialInfo credential)
        {
            Credentials.Remove(credential);
        }

















        #region Encryption / Decryption
        // Add key lenght as const

        private static string Encrypt(string plain, string password)
        {
            byte[] salt = Generate256BitsOfRandomEntropy();
            byte[] iv = Generate256BitsOfRandomEntropy();
            byte[] text = Encoding.UTF8.GetBytes(plain);

            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt))
            {
                byte[] key = rfc2898DeriveBytes.GetBytes(32); // 256 Bits / 8 Bits = 32 Bytes

                using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.BlockSize = 256;
                    rijndaelManaged.Mode = CipherMode.CBC;
                    rijndaelManaged.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = rijndaelManaged.CreateEncryptor(key, iv))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(text, 0, text.Length);
                                cryptoStream.FlushFinalBlock();

                                byte[] cipher = salt;
                                cipher = cipher.Concat(iv).ToArray();
                                cipher = cipher.Concat(memoryStream.ToArray()).ToArray();

                                memoryStream.Close();
                                cryptoStream.Close();

                                return Convert.ToBase64String(cipher);
                            }
                        }
                    }
                }
            }
        }

        private static string Decrypt(string encrypted, string password)
        {
            byte[] cipherWithSaltAndIv = Convert.FromBase64String(encrypted);
            byte[] salt = cipherWithSaltAndIv.Take(32).ToArray(); // 256 bits / 8 bits = 32 bytes
            byte[] iv = cipherWithSaltAndIv.Skip(32).Take(32).ToArray(); // Skip 32 bytes, take 32 Bytes iv
            byte[] cipher = cipherWithSaltAndIv.Skip(64).Take(cipherWithSaltAndIv.Length - 64).ToArray(); // Skip 64 bytes, take cipher bytes (length - 64)

            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt))
            {
                byte[] key = rfc2898DeriveBytes.GetBytes(32); // 256 Bits / 8 Bits = 32 Bytes

                using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
                {
                    rijndaelManaged.BlockSize = 256;
                    rijndaelManaged.Mode = CipherMode.CBC;
                    rijndaelManaged.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = rijndaelManaged.CreateDecryptor(key, iv))
                    {
                        using (MemoryStream memoryStream = new MemoryStream(cipher))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] text = new byte[cipher.Length];
                                int count = cryptoStream.Read(text, 0, text.Length);

                                memoryStream.Close();
                                cryptoStream.Close();

                                return Encoding.UTF8.GetString(text, 0, count);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.

            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }

            return randomBytes;
        }
        #endregion
    }
}
