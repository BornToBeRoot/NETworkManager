// Contains code from: https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp 

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

        // Collection with ID, Name, Username, Password
        public static ObservableCollection<CredentialInfo> Credentials = new ObservableCollection<CredentialInfo>();

        // List with ID and Name
        public static IEnumerable<CredentialInfo> CredentialInfoList
        {
            get { return Credentials.Select(x => new CredentialInfo(x.ID, x.Name)); }
        }

        public static CredentialInfo GetCredentialByID(int id)
        {
            return Credentials.FirstOrDefault(x => x.ID == id);
        }

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

        public static bool Load(SecureString pasword)
        {
            try
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

                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
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

            // Check if the path exists, create if not
            File.WriteAllBytes(GetCredentialsFilePath(), encrypted);

            CredentialsChanged = false;
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

        public static int GetNextID()
        {
            return Credentials.Count == 0 ? 0 : Credentials.OrderByDescending(x => x.ID).FirstOrDefault().ID + 1;
        }

        public static void SetMasterPassword(SecureString masterPasword)
        {
            _masterPassword = masterPasword;
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
        private const int KeySize = 256;
        private const int Iterations = 25000;

        private static byte[] Encrypt(byte[] text, string password)
        {
            byte[] salt = Generate256BitsOfRandomEntropy();
            byte[] iv = Generate256BitsOfRandomEntropy();

            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] key = rfc2898DeriveBytes.GetBytes(KeySize / 8); // 256 Bits / 8 Bits = 32 Bytes

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
            byte[] salt = cipherWithSaltAndIv.Take(KeySize / 8).ToArray(); // 256 bits / 8 bits = 32 bytes
            byte[] iv = cipherWithSaltAndIv.Skip(KeySize / 8).Take(KeySize / 8).ToArray(); // Skip 32 bytes, take 32 Bytes iv
            byte[] cipher = cipherWithSaltAndIv.Skip((KeySize / 8) * 2).Take(cipherWithSaltAndIv.Length - ((KeySize / 8) * 2)).ToArray(); // Skip 64 bytes, take cipher bytes (length - 64)

            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] key = rfc2898DeriveBytes.GetBytes(KeySize / 8); // 256 Bits / 8 Bits = 32 Bytes

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
            byte[] randomBytes = new byte[32]; // 32 * 8 = 256 Bits.

            using (RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(randomBytes);
            }

            return randomBytes;
        }
        #endregion
    }
}
