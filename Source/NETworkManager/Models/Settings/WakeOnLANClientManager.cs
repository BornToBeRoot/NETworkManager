using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class WakeOnLANClientManager
    {
        public const string ClientsFileName = "WakeOnLAN.clients";

        public static ObservableCollection<WakeOnLANClientInfo> Clients { get; set; }
        public static bool ClientsChanged { get; set; }

        public static string GetClientsFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), ClientsFileName);
        }

        public static List<string> GetClientGroups()
        {
            List<string> list = new List<string>();

            foreach (WakeOnLANClientInfo client in Clients)
            {
                if (!list.Contains(client.Group))
                    list.Add(client.Group);
            }

            return list;
        }

        public static void Load(bool deserialize = true)
        {
            Clients = new ObservableCollection<WakeOnLANClientInfo>();

            if (deserialize)
                DeserializeFromFile();

            Clients.CollectionChanged += WakeOnLANClients_CollectionChanged;
        }

        public static void Import(bool overwrite)
        {
            if (overwrite)
                Clients.Clear();

            DeserializeFromFile();
        }

        private static void DeserializeFromFile()
        {
            if (File.Exists(GetClientsFilePath()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<WakeOnLANClientInfo>));

                using (FileStream fileStream = new FileStream(GetClientsFilePath(), FileMode.Open))
                {
                    ((List<WakeOnLANClientInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(client => AddClient(client));
                }
            }
        }

        private static void WakeOnLANClients_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ClientsChanged = true;
        }

        public static void Save()
        {
            SerializeToFile();

            ClientsChanged = false;
        }

        private static void SerializeToFile()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<WakeOnLANClientInfo>));

            using (FileStream fileStream = new FileStream(GetClientsFilePath(), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<WakeOnLANClientInfo>(Clients));
            }
        }

        public static void Reset()
        {
            if (Clients == null)
            {
                Load(false);
                ClientsChanged = true;
            }
            else
            {
                Clients.Clear();
            }
        }

        public static void AddClient(WakeOnLANClientInfo client)
        {
            Clients.Add(client);
        }

        public static void RemoveClient(WakeOnLANClientInfo client)
        {
            Clients.Remove(client);
        }
    }
}
