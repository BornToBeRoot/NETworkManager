// Contains code from: https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp 

using System;
using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Threading;
using NETworkManager.Models.Cryptographie;

namespace NETworkManager.Models.Settings
{
    public static class CredentialManager
    {
        public const string CredentialsFileName = "Credentials";
        public const string CredentialsVersion = "V2";
        public const string CredentialsExtension = "xml";

        // Collection with ID, Name, Username, Password
        public static ObservableCollection<CredentialInfo> Credentials = new ObservableCollection<CredentialInfo>();

        // List with ID and Name
        public static IEnumerable<CredentialInfo> CredentialInfoList
        {
            get { return Credentials.Select(x => new CredentialInfo(x.ID, x.Name)); }
        }

        public static CredentialInfo GetCredentialByID(Guid id)
        {
            return Credentials.FirstOrDefault(x => x.ID == id);
        }

        public static bool CredentialsChanged { get; set; }

        public static bool IsLoaded { get; private set; }

        private static SecureString _masterPassword;

        public static bool VerifyMasterPasword(SecureString password)
        {
            return SecureStringHelper.ConvertToString(_masterPassword).Equals(SecureStringHelper.ConvertToString(password));
        }

        public static string GetCredentialsFileName()
        {
            return $"{CredentialsFileName}.{CredentialsVersion}.{CredentialsExtension}";
        }

        public static string GetCredentialsFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), GetCredentialsFileName());
        }

        public static bool Load(SecureString pasword)
        {
            try
            {
                byte[] xml = null;

                // Decrypt file
                if (File.Exists(GetCredentialsFilePath()))
                {
                    var cipherWithSaltAndIv = File.ReadAllBytes(GetCredentialsFilePath());

                    xml = FileCryptoHelper.Decrypt(cipherWithSaltAndIv, SecureStringHelper.ConvertToString(pasword));
                }

                // Save master pw for encryption
                SetMasterPassword(pasword);

                // Check if array is empty...
                if (xml != null && xml.Length > 0)
                    DeserializeFromByteArray(xml);

                Credentials.CollectionChanged += Credentials_CollectionChanged;

                IsLoaded = true;

                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        private static void DeserializeFromByteArray(byte[] xml)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<CredentialInfoSerializable>));

            using (var memoryStream = new MemoryStream(xml))
            {
                ((List<CredentialInfoSerializable>)(xmlSerializer.Deserialize(memoryStream))).ForEach(credential => AddCredential(new CredentialInfo(credential.ID, credential.Name, credential.Username, SecureStringHelper.ConvertToSecureString(credential.Password))));
            }
        }

        public static void Save()
        {
            // Serialize as xml (utf-8)
            byte[] credentials = SerializeToByteArray();

            // Encrypt with master pw and save file
            byte[] encrypted = FileCryptoHelper.Encrypt(credentials, SecureStringHelper.ConvertToString(_masterPassword));

            // Check if the path exists, create if not
            File.WriteAllBytes(GetCredentialsFilePath(), encrypted);

            CredentialsChanged = false;
        }

        private static byte[] SerializeToByteArray()
        {
            // Convert CredentialInfo to CredentialInfoSerializable
            var list = new List<CredentialInfoSerializable>();

            foreach (var info in Credentials)
            {
                list.Add(new CredentialInfoSerializable(info.ID, info.Name, info.Username, SecureStringHelper.ConvertToString(info.Password)));
            }

            var xmlSerializer = new XmlSerializer(typeof(List<CredentialInfoSerializable>));

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8))
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

        public static void SetMasterPassword(SecureString masterPasword)
        {
            _masterPassword = masterPasword;
        }

        public static void AddCredential(CredentialInfo credential)
        {
            // Possible fix for appcrash --> when icollection view is refreshed...           
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (Credentials)
                    Credentials.Add(credential);
            }));
        }

        public static void RemoveCredential(CredentialInfo credential)
        {
            // Possible fix for appcrash --> when icollection view is refreshed...
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                lock (Credentials)
                    Credentials.Remove(credential);
            }));
        }

        #region Encryption / Decryption

        #endregion
    }
}
