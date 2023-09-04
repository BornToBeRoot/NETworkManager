using System;
using System.Net;

namespace NETworkManager.Localization;

public static class ClipboardTranslator
{
    public static string Translate(object data)
    {
        return data switch
        {
            IPAddress ip => ip.ToString(),
            null => "-/-",
            _ => data.ToString()
        };
    }
}