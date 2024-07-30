using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Documentation;

/// <summary>
///     This class provides information's about libraries used within the program.
/// </summary>
public static class LibraryManager
{
    /// <summary>
    ///     Name of the license folder.
    /// </summary>
    private const string LicenseFolderName = "Licenses";

    /// <summary>
    ///     Static list with all libraries that are used.
    /// </summary>
    public static List<LibraryInfo> List =>
    [
        new LibraryInfo("#SNMP Library", "https://github.com/lextudio/sharpsnmplib",
            Strings.Library_SharpSNMP_Description,
            Strings.License_MITLicense,
            "https://github.com/lextudio/sharpsnmplib/blob/master/LICENSE"),
        new LibraryInfo("AirspaceFixer", "https://github.com/chris84948/AirspaceFixer",
            Strings.Library_AirspaceFixer_Description,
            Strings.License_MITLicense,
            "https://github.com/chris84948/AirspaceFixer/blob/master/LICENSE"),
        new LibraryInfo("AWSSDK.EC2", "https://github.com/aws/aws-sdk-net/",
            Strings.Library_AWSSDKdotEC2_Description,
            Strings.License_ApacheLicense2dot0, "https://aws.amazon.com/apache-2-0/"),
        new LibraryInfo("ControlzEx", "https://github.com/ControlzEx/ControlzEx",
            Strings.Library_ControlzEx_Description,
            Strings.License_MITLicense,
            "https://github.com/ButchersBoy/Dragablz/blob/master/LICENSE"),
        new LibraryInfo("DnsClient.NET", "https://github.com/MichaCo/DnsClient.NET",
            Strings.Library_DnsClientNET_Description,
            Strings.License_ApacheLicense2dot0,
            "https://github.com/MichaCo/DnsClient.NET/blob/dev/LICENSE"),
        new LibraryInfo("Dragablz", "https://github.com/ButchersBoy/Dragablz",
            Strings.Library_Dragablz_Description,
            Strings.License_MITLicense,
            "https://github.com/ButchersBoy/Dragablz/blob/master/LICENSE"),
        new LibraryInfo("GongSolutions.Wpf.DragDrop", "https://github.com/punker76/gong-wpf-dragdrop",
            Strings.Library_GongSolutionsWpfDragDrop_Description,
            Strings.License_BDS3Clause,
            "https://github.com/punker76/gong-wpf-dragdrop/blob/develop/LICENSE"),
        new LibraryInfo("IPNetwork", "https://github.com/lduchosal/ipnetwork",
            Strings.Library_IPNetwork_Description,
            Strings.License_BDS2Clause,
            "https://github.com/lduchosal/ipnetwork/blob/master/LICENSE"),
        new LibraryInfo("LiveCharts", "https://github.com/Live-Charts/Live-Charts",
            Strings.Library_LiveCharts_Description,
            Strings.License_MITLicense,
            "https://github.com/Live-Charts/Live-Charts/blob/master/LICENSE.TXT"),
        new LibraryInfo("LoadingIndicators.WPF", "https://github.com/zeluisping/LoadingIndicators.WPF",
            Strings.Library_LoadingIndicatorsWPF_Description,
            Strings.License_Unlicense,
            "https://github.com/zeluisping/LoadingIndicators.WPF/blob/master/LICENSE"),
        new LibraryInfo("log4net", "https://logging.apache.org/log4net/",
            Strings.Library_log4net_Description,
            Strings.License_ApacheLicense2dot0,
            "https://github.com/apache/logging-log4net/blob/master/LICENSE"),
        new LibraryInfo("MahApps.Metro", "https://github.com/mahapps/mahapps.metro",
            Strings.Library_MahAppsMetro_Description,
            Strings.License_MITLicense,
            "https://github.com/MahApps/MahApps.Metro/blob/master/LICENSE"),
        new LibraryInfo("MahApps.Metro.IconPacks", "https://github.com/MahApps/MahApps.Metro.IconPacks",
            Strings.Library_MahAppsMetroIconPacks_Description,
            Strings.License_MITLicense,
            "https://github.com/MahApps/MahApps.Metro.IconPacks/blob/master/LICENSE"),
        new LibraryInfo("Microsoft.PowerShell.SDK", "https://github.com/PowerShell/PowerShell",
            Strings.Library_PowerShellSDK_Description,
            Strings.License_MITLicense,
            "https://github.com/PowerShell/PowerShell/blob/master/LICENSE.txt"),
        new LibraryInfo("Microsoft.Web.WebView2", "https://docs.microsoft.com/en-us/microsoft-edge/webview2/",
            Strings.Library_WebView2_Description,
            Strings.License_MicrosoftWebView2License,
            "https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.824-prerelease/License"),
        new LibraryInfo("Microsoft.Windows.CsWinRT", "https://github.com/microsoft/cswinrt/tree/master/",
            Strings.Library_CsWinRT_Description,
            Strings.License_MITLicense,
            "https://github.com/microsoft/CsWinRT/blob/master/LICENSE"),
        new LibraryInfo("Microsoft.Xaml.Behaviors.Wpf", "https://github.com/microsoft/XamlBehaviorsWpf",
            Strings.Library_XamlBehaviorsWpf_Description,
            Strings.License_MITLicense,
            "https://github.com/microsoft/XamlBehaviorsWpf/blob/master/LICENSE"),
        new LibraryInfo("Newtonsoft.Json", "https://github.com/JamesNK/Newtonsoft.Json",
            Strings.Library_NewtonsoftJson_Description,
            Strings.License_MITLicense,
            "https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md"),
        new LibraryInfo("nulastudio.NetBeauty", "https://github.com/nulastudio/NetBeauty2",
            Strings.Library_nulastudioNetBeauty_Description,
            Strings.License_MITLicense,
            "https://github.com/nulastudio/NetBeauty2/blob/master/LICENSE"),
        new LibraryInfo("Octokit", "https://github.com/octokit/octokit.net",
            Strings.Library_Octokit_Description,
            Strings.License_MITLicense,
            "https://github.com/octokit/octokit.net/blob/master/LICENSE.txt"),
        new LibraryInfo("PSDiscoveryProtocol", "https://github.com/lahell/PSDiscoveryProtocol",
            Strings.Library_PSDicoveryProtocol_Description,
            Strings.License_MITLicense,
            "https://github.com/lahell/PSDiscoveryProtocol/blob/master/LICENSE")
    ];


    /// <summary>
    ///     Method to get the license folder location.
    /// </summary>
    /// <returns>Location of the license folder.</returns>
    public static string GetLicenseLocation()
    {
        return Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
            throw new DirectoryNotFoundException(
                "Program execution directory not found, while trying to build path to license directory!"),
            LicenseFolderName);
    }
}