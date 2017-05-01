using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;
using System.Xml;
using System.IO;
using NETworkManager.Settings;

// Source: http://www.codeproject.com/Articles/20917/Creating-a-Custom-Settings-Provider?msg=2934144#xx2934144xx

public class SettingsProviderPortable : SettingsProvider
{
    const string XmlSettingsRoot = "Settings";

    public override void Initialize(string name, NameValueCollection collection)
    {
        base.Initialize(ApplicationName, collection);
    }

    public override string ApplicationName
    {
        get { return Assembly.GetEntryAssembly().GetName().Name; }
        set { }
    }

    public override string Name
    {
        get { return "CustomSettingsProvider"; }
    }

    public virtual string GetAppSettingsPath()
    {
        return SettingsManager.SettingsLocation;
    }

    public virtual string GetAppSettingsFilename()
    {
        //Used to determine the filename to store the settings
        return ApplicationName + ".settings";
    }

    public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values)
    {
        //Iterate through the settings to be stored
        //Only dirty settings are included in propvals, and only ones relevant to this provider
        foreach (SettingsPropertyValue value in values)
        {
            SetValue(value);
        }

        try
        {
            SettingsXMLDocument.Save(Path.Combine(GetAppSettingsPath(), GetAppSettingsFilename()));
        }
        catch
        {

        }
        //Ignore if cant save, device been ejected
    }

    public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection settings)
    {
        //Create new collection of values
        SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

        //Iterate through the settings to be retrieved
        foreach (SettingsProperty setting in settings)
        {
            SettingsPropertyValue value = new SettingsPropertyValue(setting);
            value.IsDirty = false;
            value.SerializedValue = GetValue(setting);
            values.Add(value);
        }
        return values;
    }

    private XmlDocument _settingsXMLDocument = null;

    private XmlDocument SettingsXMLDocument
    {
        get
        {
            //If we dont hold an xml document, try opening one.  
            //If it doesnt exist then create a new one ready.
            if (_settingsXMLDocument == null)
            {
                _settingsXMLDocument = new XmlDocument();

                try
                {
                    _settingsXMLDocument.Load(Path.Combine(GetAppSettingsPath(), GetAppSettingsFilename()));
                }
                catch 
                {
                    //Create new document
                    XmlDeclaration declaration = _settingsXMLDocument.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                    _settingsXMLDocument.AppendChild(declaration);

                    XmlNode nodeRoot = default(XmlNode);

                    nodeRoot = _settingsXMLDocument.CreateNode(XmlNodeType.Element, XmlSettingsRoot, "");
                    _settingsXMLDocument.AppendChild(nodeRoot);
                }
            }

            return _settingsXMLDocument;
        }
    }

    private string GetValue(SettingsProperty setting)
    {
        string result = "";

        try
        {
            if (IsRoaming(setting))
            {
                result = SettingsXMLDocument.SelectSingleNode(XmlSettingsRoot + "/" + setting.Name).InnerText;
            }
            else
            {
                result = SettingsXMLDocument.SelectSingleNode(XmlSettingsRoot + "/" + Environment.MachineName + "/" + setting.Name).InnerText;
            }
        }

        catch
        {
            if ((setting.DefaultValue != null))
            {
                result = setting.DefaultValue.ToString();
            }
            else
            {
                result = "";
            }
        }

        return result;
    }

    private void SetValue(SettingsPropertyValue value)
    {

        XmlElement MachineNode = default(XmlElement);
        XmlElement SettingNode = default(XmlElement);

        //Determine if the setting is roaming.
        //If roaming then the value is stored as an element under the root
        //Otherwise it is stored under a machine name node 
        try
        {
            if (IsRoaming(value.Property))
            {
                SettingNode = (XmlElement)SettingsXMLDocument.SelectSingleNode(XmlSettingsRoot + "/" + value.Name);
            }
            else
            {
                SettingNode = (XmlElement)SettingsXMLDocument.SelectSingleNode(XmlSettingsRoot + "/" + Environment.MachineName + "/" + value.Name);
            }
        }
        catch
        {
            SettingNode = null;
        }

        //Check to see if the node exists, if so then set its new value
        if ((SettingNode != null))
        {
            SettingNode.InnerText = value.SerializedValue.ToString();
        }
        else
        {
            if (IsRoaming(value.Property))
            {
                //Store the value as an element of the Settings Root Node
                SettingNode = SettingsXMLDocument.CreateElement(value.Name);
                SettingNode.InnerText = value.SerializedValue.ToString();
                SettingsXMLDocument.SelectSingleNode(XmlSettingsRoot).AppendChild(SettingNode);
            }
            else
            {
                //Its machine specific, store as an element of the machine name node,
                //creating a new machine name node if one doesnt exist.
                try
                {

                    MachineNode = (XmlElement)SettingsXMLDocument.SelectSingleNode(XmlSettingsRoot + "/" + Environment.MachineName);
                }
                catch
                {
                    MachineNode = SettingsXMLDocument.CreateElement(Environment.MachineName);
                    SettingsXMLDocument.SelectSingleNode(XmlSettingsRoot).AppendChild(MachineNode);
                }

                if (MachineNode == null)
                {
                    MachineNode = SettingsXMLDocument.CreateElement(Environment.MachineName);
                    SettingsXMLDocument.SelectSingleNode(XmlSettingsRoot).AppendChild(MachineNode);
                }

                SettingNode = SettingsXMLDocument.CreateElement(value.Name);
                SettingNode.InnerText = value.SerializedValue.ToString();
                MachineNode.AppendChild(SettingNode);
            }
        }
    }

    private bool IsRoaming(SettingsProperty properties)
    {
        //Determine if the setting is marked as Roaming
        foreach (DictionaryEntry entry in properties.Attributes)
        {
            Attribute attribute = (Attribute)entry.Value;
            if (attribute is SettingsManageabilityAttribute)
            {
                return true;
            }
        }

        return false;
    }
}