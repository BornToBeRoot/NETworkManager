using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace NETworkManager.Models.Settings
{
    public static class RemoteDesktopSessionManager
    {
        public const string SessionsFileName = "RemoteDesktop.sessions";

        public static ObservableCollection<RemoteDesktopSessionInfo> Sessions { get; set; }
        public static bool SessionsChanged { get; set; }

        public static string GetSessionsFilePath()
        {
            return Path.Combine(SettingsManager.GetSettingsLocation(), SessionsFileName);
        }

        public static List<string> GetSessionGroups()
        {
            List<string> list = new List<string>();

            foreach (RemoteDesktopSessionInfo session in Sessions)
            {
                if (!list.Contains(session.Group))
                    list.Add(session.Group);
            }

            return list;
        }

        public static void Load(bool deserialize = true)
        {
            Sessions = new ObservableCollection<RemoteDesktopSessionInfo>();

            if (deserialize)
                Deserialize().ForEach(session => AddSession(session));

            Sessions.CollectionChanged += Sessions_CollectionChanged; ;
        }

        public static void Import(bool overwrite)
        {
            if (overwrite)
                Sessions.Clear();

            Deserialize().ForEach(session => AddSession(session));
        }

        private static List<RemoteDesktopSessionInfo> Deserialize()
        {
            List<RemoteDesktopSessionInfo> list = new List<RemoteDesktopSessionInfo>();

            if (File.Exists(GetSessionsFilePath()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<RemoteDesktopSessionInfo>));

                using (FileStream fileStream = new FileStream(GetSessionsFilePath(), FileMode.Open))
                {
                    ((List<RemoteDesktopSessionInfo>)(xmlSerializer.Deserialize(fileStream))).ForEach(session => list.Add(session));
                }
            }

            return list;
        }

        private static void Sessions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SessionsChanged = true;
        }

        public static void Save()
        {
            Serialize();

            SessionsChanged = false;
        }

        private static void Serialize()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<RemoteDesktopSessionInfo>));

            using (FileStream fileStream = new FileStream(GetSessionsFilePath(), FileMode.Create))
            {
                xmlSerializer.Serialize(fileStream, new List<RemoteDesktopSessionInfo>(Sessions));
            }
        }

        public static void Reset()
        {
            if (Sessions == null)
                Sessions = new ObservableCollection<RemoteDesktopSessionInfo>();
            else
                Sessions.Clear();
        }

        public static void AddSession(RemoteDesktopSessionInfo session)
        {
            Sessions.Add(session);
        }

        public static void RemoveSession(RemoteDesktopSessionInfo session)
        {
            Sessions.Remove(session);
        }
    }
}
