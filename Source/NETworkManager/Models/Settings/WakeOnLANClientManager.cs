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

        public static void Load()
        {
            Clients = new ObservableCollection<WakeOnLANClientInfo>();

            Deserialize().ForEach(client => AddClient(client));

            Clients.CollectionChanged += WakeOnLANClients_CollectionChanged;
        }

        public static void Reload()
        {
            Clients.Clear();

            Deserialize().ForEach(client => AddClient(client));
        }

        private static List<WakeOnLANClientInfo> Deserialize()
        {
            List<WakeOnLANClientInfo> list = new List<WakeOnLANClientInfo>();

            if (File.Exists(GetClientsFilePath()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<WakeOnLANClientInfo>));

                using (FileStream fileStream = new FileStream(GetClientsFilePath(), FileMode.Open))
                {
                    ((List<WakeOnLANClientInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(client => list.Add(client));
                }
            }

            return list;
        }

        private static void WakeOnLANClients_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ClientsChanged = true;
        }

        public static void Save()
        {
            Serialize();

            ClientsChanged = false;
        }

        private static void Serialize()
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
                Clients = new ObservableCollection<WakeOnLANClientInfo>();
            else
                Clients.Clear();
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
