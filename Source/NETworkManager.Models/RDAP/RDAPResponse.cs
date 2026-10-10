using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     RDAP object (RFC 9083) as returned by a server. One class for all object classes (domain, ip network, autnum,
///     entity, nameserver), since they share most members. Every member is optional, because real servers omit
///     almost anything. Unknown members (vendor extensions) are ignored; the raw JSON is kept in
///     <see cref="RDAPResult" />.
/// </summary>
public class RDAPObject
{
    // Common members
    [JsonProperty("objectClassName")] public string ObjectClassName { get; set; }
    [JsonProperty("handle")] public string Handle { get; set; }
    [JsonProperty("status")] public List<string> Status { get; set; }
    [JsonProperty("events")] public List<RDAPEvent> Events { get; set; }
    [JsonProperty("links")] public List<RDAPLink> Links { get; set; }
    [JsonProperty("remarks")] public List<RDAPNotice> Remarks { get; set; }
    [JsonProperty("entities")] public List<RDAPObject> Entities { get; set; }
    [JsonProperty("port43")] public string Port43 { get; set; }
    [JsonProperty("lang")] public string Lang { get; set; }
    [JsonProperty("publicIds")] public List<RDAPPublicId> PublicIds { get; set; }

    // Domain and nameserver
    [JsonProperty("ldhName")] public string LdhName { get; set; }
    [JsonProperty("unicodeName")] public string UnicodeName { get; set; }
    [JsonProperty("nameservers")] public List<RDAPObject> Nameservers { get; set; }
    [JsonProperty("secureDNS")] public RDAPSecureDNS SecureDNS { get; set; }
    [JsonProperty("ipAddresses")] public RDAPIPAddresses IPAddresses { get; set; }

    // IP network and autnum
    [JsonProperty("startAddress")] public string StartAddress { get; set; }
    [JsonProperty("endAddress")] public string EndAddress { get; set; }
    [JsonProperty("ipVersion")] public string IPVersion { get; set; }
    [JsonProperty("startAutnum")] public long? StartAutnum { get; set; }
    [JsonProperty("endAutnum")] public long? EndAutnum { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("country")] public string Country { get; set; }
    [JsonProperty("parentHandle")] public string ParentHandle { get; set; }
    [JsonProperty("cidr0_cidrs")] public List<RDAPCidr> Cidrs { get; set; }

    // Entity
    [JsonProperty("roles")] public List<string> Roles { get; set; }

    /// <summary>
    ///     jCard (RFC 7095). Kept as raw JSON, since its shape varies between servers. See <see cref="JCardParser" />.
    /// </summary>
    [JsonProperty("vcardArray")]
    public JToken VCardArray { get; set; }
}

/// <summary>
///     Top-level RDAP response, which additionally carries notices and the error members (RFC 9083 4.1, 6).
/// </summary>
public class RDAPResponse : RDAPObject
{
    [JsonProperty("rdapConformance")] public List<string> RDAPConformance { get; set; }
    [JsonProperty("notices")] public List<RDAPNotice> Notices { get; set; }

    // Error response
    [JsonProperty("errorCode")] public int? ErrorCode { get; set; }
    [JsonProperty("title")] public string Title { get; set; }
    [JsonProperty("description")] public List<string> Description { get; set; }
}

public class RDAPEvent
{
    [JsonProperty("eventAction")] public string EventAction { get; set; }
    [JsonProperty("eventActor")] public string EventActor { get; set; }

    /// <summary>
    ///     Event date as sent by the server. Parse with <see cref="RDAPDateParser" />.
    /// </summary>
    [JsonProperty("eventDate")]
    public string EventDate { get; set; }
}

public class RDAPLink
{
    [JsonProperty("value")] public string Value { get; set; }
    [JsonProperty("rel")] public string Rel { get; set; }
    [JsonProperty("href")] public string Href { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("title")] public string Title { get; set; }
}

public class RDAPNotice
{
    [JsonProperty("title")] public string Title { get; set; }
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("description")] public List<string> Description { get; set; }
    [JsonProperty("links")] public List<RDAPLink> Links { get; set; }
}

public class RDAPPublicId
{
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("identifier")] public string Identifier { get; set; }
}

public class RDAPIPAddresses
{
    [JsonProperty("v4")] public List<string> V4 { get; set; }
    [JsonProperty("v6")] public List<string> V6 { get; set; }
}

public class RDAPCidr
{
    [JsonProperty("v4prefix")] public string V4Prefix { get; set; }
    [JsonProperty("v6prefix")] public string V6Prefix { get; set; }
    [JsonProperty("length")] public int? Length { get; set; }
}

public class RDAPSecureDNS
{
    [JsonProperty("zoneSigned")] public bool? ZoneSigned { get; set; }
    [JsonProperty("delegationSigned")] public bool? DelegationSigned { get; set; }
    [JsonProperty("maxSigLife")] public long? MaxSigLife { get; set; }
    [JsonProperty("dsData")] public List<RDAPDSData> DSData { get; set; }
    [JsonProperty("keyData")] public List<RDAPKeyData> KeyData { get; set; }
}

public class RDAPDSData
{
    [JsonProperty("keyTag")] public long? KeyTag { get; set; }
    [JsonProperty("algorithm")] public int? Algorithm { get; set; }
    [JsonProperty("digest")] public string Digest { get; set; }
    [JsonProperty("digestType")] public int? DigestType { get; set; }
}

public class RDAPKeyData
{
    [JsonProperty("flags")] public int? Flags { get; set; }
    [JsonProperty("protocol")] public int? Protocol { get; set; }
    [JsonProperty("publicKey")] public string PublicKey { get; set; }
    [JsonProperty("algorithm")] public int? Algorithm { get; set; }
}
