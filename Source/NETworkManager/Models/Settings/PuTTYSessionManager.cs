using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class PuTTYSessionManager
    {
        public const string SessionsFileName = "PuTTY.sessions";

        public static ObservableCollection<PuTTYSessionInfo> Sessions { get; set; }
        public static bool SessionsChanged { get; set; }

        public static string GetSessionsFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), SessionsFileName);
        }

        public static List<string> GetSessionGroups()
        {
            List<string> list = new List<string>();

            foreach (PuTTYSessionInfo session in Sessions)
            {
                if (!list.Contains(session.Group))
                    list.Add(session.Group);
            }

            return list;
        }

        public static void Load(bool deserialize = true)
        {
            Sessions = new ObservableCollection<PuTTYSessionInfo>();

            if (deserialize)
                DeserializeFromFile();

            Sessions.CollectionChanged += Sessions_CollectionChanged; ;
        }

        public static void Import(bool overwrite)
        {
            if (overwrite)
                Sessions.Clear();

            DeserializeFromFile();
        }

        private static void DeserializeFromFile()
        {
            if (File.Exists(GetSessionsFilePath()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PuTTYSessionInfo>));

                using (FileStream fileStream = new FileStream(GetSessionsFilePath(), FileMode.Open))
                {
                    ((List<PuTTYSessionInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(session => AddSession(session));
                }
            }
        }

        private static void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SessionsChanged = true;
        }

        public static void Save()
        {
            SerializeToFile();

            SessionsChanged = false;
        }

        private static void SerializeToFile()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<PuTTYSessionInfo>));

            using (FileStream fileStream = new FileStream(GetSessionsFilePath(), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<PuTTYSessionInfo>(Sessions));
            }
        }

        public static void Reset()
        {
            if (Sessions == null)
            {
                Load(false);
                SessionsChanged = true;
            }
            else
            {
                Sessions.Clear();
            }
        }

        public static void AddSession(PuTTYSessionInfo session)
        {
            Sessions.Add(session);
        }

        public static void RemoveSession(PuTTYSessionInfo session)
        {
            Sessions.Remove(session);
        }
    }
}
