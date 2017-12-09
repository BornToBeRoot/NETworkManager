// Contains code from: https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp 
// and: https://www.vb-paradise.de/index.php/Thread/81625-verschluesseln-und-autentifizieren/#post668740

using NETworkManager.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class CredentialManager
    {
        public const string CredentialsFileName = "Credentials.encrypted";

        public static ObservableCollection<CredentialInfo> Credentials { get; set; }
        public static bool CredentialsChanged { get; set; }

        private static bool _loaded { get; set; }
        public static bool Loaded
        {
            get { return _loaded; }
        }

        private static SecureString _masterPassword;

        public static bool VerifyMasterPasword(SecureString password)
        {
            return SecureStringHelper.ConvertToString(_masterPassword).Equals(SecureStringHelper.ConvertToString(password));
        }

        public static string GetCredentialsFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), CredentialsFileName);
        }

        public static void SetMasterPassword(SecureString masterPasword)
        {
            _masterPassword = masterPasword;
        }

        public static void Load(SecureString pasword)
        {
            byte[] xml = null;

            // Decrypt file
            if (File.Exists(GetCredentialsFilePath()))
            {
                byte[] cipherWithSaltAndIv = File.ReadAllBytes(GetCredentialsFilePath());

                xml = Decrypt(cipherWithSaltAndIv, SecureStringHelper.ConvertToString(pasword));
            }

            // Save master pw for encryption
            SetMasterPassword(pasword);

            // Init collection
            Credentials = new ObservableCollection<CredentialInfo>();

            // Check if array is empty...
            if (xml != null && xml.Length > 0)
            {
                foreach (CredentialInfoSerializable info in Deserialize(xml))
                {
                    AddCredential(new CredentialInfo(info.ID, info.Name, info.Username, SecureStringHelper.ConvertToSecureString(info.Password)));
                }
            }

            Credentials.CollectionChanged += Credentials_CollectionChanged;

            _loaded = true;
        }

        private static List<CredentialInfoSerializable> Deserialize(byte[] xml)
        {
            List<CredentialInfoSerializable> list = new List<CredentialInfoSerializable>();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CredentialInfoSerializable>));

            using (MemoryStream memoryStream = new MemoryStream(xml))
            {
                ((List<CredentialInfoSerializable>)(xmlSerializer.Deserialize(memoryStream))).ForEach(credential => list.Add(credential));
            }

            return list;
        }

        public static void Save()
        {
            // Convert CredentialInfo to CredentialInfoSerializable
            List<CredentialInfoSerializable> list = new List<CredentialInfoSerializable>();

            foreach (CredentialInfo info in Credentials)
            {
                list.Add(new CredentialInfoSerializable(info.ID, info.Name, info.Username, SecureStringHelper.ConvertToString(info.Password)));
            }

            // Serialize as xml (utf-8)
            byte[] credentials = Serialize(list);

            // Encrypt with master pw and save file
            byte[] encrypted = Encrypt(credentials, SecureStringHelper.ConvertToString(_masterPassword));

            File.WriteAllBytes(GetCredentialsFilePath(), encrypted);
        }

        private static byte[] Serialize(List<CredentialInfoSerializable> list)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CredentialInfoSerializable>));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    xmlSerializer.Serialize(streamWriter, list);
                    return memoryStream.ToArray();
                }
            }
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

        private static byte[] Encrypt(byte[] text, string password)
        {
            byte[] salt = Generate256BitsOfRandomEntropy();
            byte[] iv = Generate256BitsOfRandomEntropy();

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

                                return cipher;
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Decrypt(byte[] cipherWithSaltAndIv, string password)
        {
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

                                return text;
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
