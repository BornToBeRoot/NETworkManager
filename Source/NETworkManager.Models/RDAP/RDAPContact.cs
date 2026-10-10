using System.Collections.Generic;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Contact information of an RDAP entity, independent of the format sent by the server (jCard today, JSContact in
///     the future). Every member is optional.
/// </summary>
public class RDAPContact
{
    /// <summary>
    ///     Kind of the contact (e.g. "individual", "org"), from the vCard "kind" property.
    /// </summary>
    public string Kind { get; set; }

    /// <summary>
    ///     Formatted name. Can be empty if the name does not exist or is redacted (RFC 9083 3).
    /// </summary>
    public string Name { get; set; }

    public string Organization { get; set; }

    public string Title { get; set; }

    /// <summary>
    ///     vCard "role" property. Not to be confused with the roles of the RDAP entity.
    /// </summary>
    public string Role { get; set; }

    public List<RDAPContactValue> Emails { get; } = [];

    public List<RDAPContactValue> Phones { get; } = [];

    /// <summary>
    ///     Postal addresses, each as a list of lines.
    /// </summary>
    public List<List<string>> Addresses { get; } = [];

    /// <summary>
    ///     URIs to contact a redacted contact, e.g. a web form (RFC 8605 "contact-uri").
    /// </summary>
    public List<string> ContactUris { get; } = [];

    public List<string> Urls { get; } = [];
}

/// <summary>
///     Value of a contact property with its vCard "type" parameter values (e.g. "voice", "fax", "work").
/// </summary>
/// <param name="Value">Value of the property.</param>
/// <param name="Types">Values of the "type" parameter.</param>
public record RDAPContactValue(string Value, IReadOnlyList<string> Types);
