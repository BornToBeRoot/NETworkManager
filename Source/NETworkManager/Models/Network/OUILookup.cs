using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using NETworkManager.Models.Settings;
using System.Text.RegularExpressions;

namespace NETworkManager.Models.Network
{
    public static class OUILookup
    {
        #region Variables
        private static string OUIFilePath = Path.Combine(ConfigurationManager.Current.StartupPath, "Resources", "oui.txt");

        public static Lookup<string, OUIInfo> OUIs;

        #endregion

        #region Methods
        static OUILookup()
        {
            List<OUIInfo> ouiList = new List<OUIInfo>();

            // Load list from resource folder (.txt-file)
            foreach (string line in File.ReadAllLines(OUIFilePath))
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] ouiData = line.Split('|');

                ouiList.Add(new OUIInfo(ouiData[0], ouiData[1]));
            }

            OUIs = (Lookup<string, OUIInfo>)ouiList.ToLookup(x => x.MACAddress);
        }

        public static Task<List<OUIInfo>> LookupAsync(string macAddressOrFirst3Bytes)
        {
            return Task.Run(() => Lookup(macAddressOrFirst3Bytes));
        }

        public static List<OUIInfo> Lookup(string macAddressOrFirst3Bytes)
        {
            List<OUIInfo> list = new List<OUIInfo>();

            string ouiKey = Regex.Replace(macAddressOrFirst3Bytes, "-|:", "").Substring(0, 6);

            foreach (OUIInfo info in OUIs[ouiKey])
            {
                list.Add(info);
            }

            return list;
        }
        #endregion
    }
}
