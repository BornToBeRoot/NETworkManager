using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NETworkManager.Models.RDAP;

/// <summary>
///     Parser for jCards (RFC 7095) as used by RDAP entities. A jCard is ["vcard", [[name, params, type, value...], ...]].
///     Real servers differ a lot, so nothing is assumed about the shape of a property:
///     - "adr" is either sent as structured components or only as a "label" parameter (often with empty components).
///     - The "type" parameter is a string or an array.
///     - "tel" is sent as "text" or as "uri" ("tel:+1.234").
///     - Structured components may themselves be arrays.
/// </summary>
public static class JCardParser
{
    /// <summary>
    ///     Parses the jCard of an RDAP entity.
    /// </summary>
    /// <param name="vcardArray">Value of the "vcardArray" member.</param>
    /// <returns>Parsed contact or null if there is no valid jCard.</returns>
    public static RDAPContact Parse(JToken vcardArray)
    {
        if (vcardArray is not JArray { Count: >= 2 } jCard || jCard[1] is not JArray properties)
            return null;

        var contact = new RDAPContact();

        foreach (var property in properties.OfType<JArray>())
        {
            if (property.Count < 4)
                continue;

            var name = property[0].ToString().ToLowerInvariant();
            var parameters = property[1] as JObject;
            var valueType = property[2].ToString().ToLowerInvariant();
            var values = property.Skip(3).ToList();

            switch (name)
            {
                case "fn":
                    contact.Name = GetText(values);
                    break;
                case "kind":
                    contact.Kind = GetText(values);
                    break;
                case "org":
                    contact.Organization = string.Join(", ", Flatten(values).Where(x => x.Length > 0));
                    break;
                case "title":
                    contact.Title = GetText(values);
                    break;
                case "role":
                    contact.Role = GetText(values);
                    break;
                case "email":
                    AddValue(contact.Emails, GetText(values), parameters);
                    break;
                case "tel":
                    var phone = GetText(values);

                    if (valueType == "uri" && phone.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
                        phone = phone[4..];

                    AddValue(contact.Phones, phone, parameters);
                    break;
                case "adr":
                    var address = ParseAddress(values.FirstOrDefault(), parameters);

                    if (address.Count > 0)
                        contact.Addresses.Add(address);
                    break;
                case "contact-uri":
                    AddText(contact.ContactUris, GetText(values));
                    break;
                case "url":
                    AddText(contact.Urls, GetText(values));
                    break;
            }
        }

        return contact;
    }

    /// <summary>
    ///     Returns the values of the "type" parameter, which can be a string or an array.
    /// </summary>
    private static List<string> GetTypes(JObject parameters)
    {
        var type = parameters?["type"];

        return type switch
        {
            JArray array => array.Select(x => x.ToString()).ToList(),
            null => [],
            _ => [type.ToString()]
        };
    }

    private static string GetText(List<JToken> values)
    {
        return string.Join(", ", Flatten(values).Where(x => x.Length > 0));
    }

    /// <summary>
    ///     Flattens values and nested arrays into a list of strings.
    /// </summary>
    private static IEnumerable<string> Flatten(IEnumerable<JToken> values)
    {
        foreach (var value in values)
        {
            if (value is JArray array)
            {
                foreach (var item in Flatten(array))
                    yield return item;
            }
            else if (value.Type != JTokenType.Null)
            {
                yield return value.ToString().Trim();
            }
        }
    }

    private static void AddValue(List<RDAPContactValue> list, string value, JObject parameters)
    {
        if (!string.IsNullOrWhiteSpace(value))
            list.Add(new RDAPContactValue(value, GetTypes(parameters)));
    }

    private static void AddText(List<string> list, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            list.Add(value);
    }

    /// <summary>
    ///     Parses an "adr" property. The "label" parameter is preferred, since it contains the address as formatted by
    ///     the registry, and many servers send it with empty structured components. Otherwise, the structured
    ///     components (post office box, extended address, street, locality, region, postal code, country) are used,
    ///     with the RFC 8605 "cc" parameter as country fallback.
    /// </summary>
    private static List<string> ParseAddress(JToken value, JObject parameters)
    {
        var label = parameters?["label"]?.ToString();

        if (!string.IsNullOrWhiteSpace(label))
            return label.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

        var lines = new List<string>();

        if (value is not JArray components)
        {
            AddText(lines, value?.ToString());
            return lines;
        }

        string Component(int index)
        {
            return index < components.Count
                ? string.Join(", ", Flatten([components[index]]).Where(x => x.Length > 0))
                : string.Empty;
        }

        // Post office box, extended address and street can contain multiple lines (nested arrays).
        for (var i = 0; i <= 2; i++)
        {
            if (i < components.Count)
                lines.AddRange(Flatten([components[i]]).Where(x => x.Length > 0));
        }

        AddText(lines, string.Join(" ", new[] { Component(5), Component(3) }.Where(x => x.Length > 0)));
        AddText(lines, Component(4));

        var country = Component(6);

        if (string.IsNullOrEmpty(country))
            country = parameters?["cc"]?.ToString();

        AddText(lines, country);

        return lines;
    }
}
