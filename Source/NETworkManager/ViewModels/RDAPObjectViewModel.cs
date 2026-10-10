using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.RDAP;

namespace NETworkManager.ViewModels;

/// <summary>
///     Display model of an RDAP response (registry or registrar) for the RDAP view and the text export.
/// </summary>
public class RDAPObjectViewModel
{
    /// <summary>
    ///     Creates the display model of an RDAP result.
    /// </summary>
    /// <param name="result">RDAP result.</param>
    /// <param name="title">Title of the result (e.g. "Registry").</param>
    public RDAPObjectViewModel(RDAPResult result, string title)
    {
        Title = title;
        RequestUrl = result.RequestUrl;
        FormattedJson = result.FormattedJson;

        var response = result.Response;

        AddDetails(response);

        Events = response.Events?
            .Select(x => new RDAPEventItem(x.EventAction, RDAPDateParser.ToLocalString(x.EventDate), x.EventActor))
            .ToList() ?? [];

        Nameservers = response.Nameservers?
            .Select(x => new RDAPNameserverItem(
                FormatDomain(x.LdhName, x.UnicodeName),
                string.Join(", ", (x.IPAddresses?.V4 ?? []).Concat(x.IPAddresses?.V6 ?? []))))
            .ToList() ?? [];

        DSData = response.SecureDNS?.DSData?
            .Select(x => new RDAPDSDataItem(x.KeyTag?.ToString(CultureInfo.InvariantCulture),
                x.Algorithm?.ToString(CultureInfo.InvariantCulture),
                x.DigestType?.ToString(CultureInfo.InvariantCulture), x.Digest))
            .ToList() ?? [];

        Contacts = [];
        AddContacts(response.Entities);

        // An entity query returns the contact itself.
        if (response.ObjectClassName == "entity")
            Contacts.Insert(0, CreateContact(response));

        Notices = (response.Notices ?? []).Concat(response.Remarks ?? [])
            .Select(x => new RDAPNoticeItem(x.Title,
                string.Join(Environment.NewLine, x.Description?.Where(d => !string.IsNullOrWhiteSpace(d)) ?? []),
                GetLinks(x.Links)))
            .ToList();
    }

    #region Properties

    public string Title { get; }

    public string RequestUrl { get; }

    public string FormattedJson { get; }

    public List<RDAPDetailItem> Details { get; } = [];

    public List<RDAPEventItem> Events { get; }

    public List<RDAPNameserverItem> Nameservers { get; }

    public List<RDAPDSDataItem> DSData { get; }

    public List<RDAPContactItem> Contacts { get; }

    public List<RDAPNoticeItem> Notices { get; }

    #endregion

    #region Methods

    private void AddDetails(RDAPResponse response)
    {
        AddDetail(Strings.Server, RequestUrl);
        AddDetail(Strings.ObjectClass, response.ObjectClassName);
        AddDetail(Strings.Handle, response.Handle);
        AddDetail(Strings.Domain, FormatDomain(response.LdhName, response.UnicodeName));
        AddDetail(Strings.Name, response.Name);
        AddDetail(Strings.Type, response.Type);

        if (response.StartAddress != null || response.EndAddress != null)
            AddDetail(Strings.Range, $"{response.StartAddress} - {response.EndAddress}");

        if (response.Cidrs is { Count: > 0 })
            AddDetail(Strings.CIDR, string.Join(", ",
                response.Cidrs.Select(x => $"{x.V4Prefix ?? x.V6Prefix}/{x.Length}")));

        if (response.StartAutnum != null || response.EndAutnum != null)
            AddDetail(Strings.Range, response.StartAutnum == response.EndAutnum
                ? $"AS{response.StartAutnum}"
                : $"AS{response.StartAutnum} - AS{response.EndAutnum}");

        AddDetail(Strings.Country, response.Country);
        AddDetail(Strings.ParentHandle, response.ParentHandle);

        if (response.Status is { Count: > 0 })
            AddDetail(Strings.Status, string.Join(", ", response.Status));

        if (response.SecureDNS?.DelegationSigned != null)
            AddDetail(Strings.DelegationSigned, response.SecureDNS.DelegationSigned.Value ? Strings.Yes : Strings.No);

        if (response.PublicIds is { Count: > 0 })
            AddDetail(Strings.PublicIDs, string.Join(", ", response.PublicIds.Select(x => $"{x.Type}: {x.Identifier}")));

        AddDetail(Strings.Port43, response.Port43);

        if (response.RDAPConformance is { Count: > 0 })
            AddDetail(Strings.Conformance, string.Join(", ", response.RDAPConformance));
    }

    private void AddDetail(string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            Details.Add(new RDAPDetailItem(name, value));
    }

    /// <summary>
    ///     Adds all entities, including nested ones (e.g. the abuse contact of a registrar).
    /// </summary>
    private void AddContacts(List<RDAPObject> entities)
    {
        foreach (var entity in entities ?? [])
        {
            Contacts.Add(CreateContact(entity));
            AddContacts(entity.Entities);
        }
    }

    private static RDAPContactItem CreateContact(RDAPObject entity)
    {
        var contact = JCardParser.Parse(entity.VCardArray) ?? new RDAPContact();

        static string FormatValues(IEnumerable<RDAPContactValue> values)
        {
            return string.Join(Environment.NewLine, values.Select(x =>
                x.Types.Any(t => t.Equals("fax", StringComparison.OrdinalIgnoreCase)) ? $"{x.Value} ({Strings.Fax})" : x.Value));
        }

        var email = FormatValues(contact.Emails);

        // Redacted contacts are often only reachable via a web form or an anonymized URI (RFC 8605).
        if (string.IsNullOrEmpty(email))
            email = string.Join(Environment.NewLine, contact.ContactUris);

        return new RDAPContactItem(
            string.Join(", ", entity.Roles ?? []),
            entity.Handle,
            string.IsNullOrWhiteSpace(contact.Name) ? null : contact.Name,
            contact.Organization,
            email,
            FormatValues(contact.Phones),
            string.Join(Environment.NewLine + Environment.NewLine,
                contact.Addresses.Select(x => string.Join(Environment.NewLine, x))));
    }

    /// <summary>
    ///     Gets the links of a notice or remark (e.g. terms of service, privacy policy, inaccuracy report). The URLs are
    ///     sent by the RDAP server and therefore untrusted: only absolute HTTP(S) URLs are used, in their normalized
    ///     form (<see cref="Uri.AbsoluteUri" /> escapes characters like "|", "^" or "%"), so they can be opened safely.
    /// </summary>
    private static List<RDAPLinkItem> GetLinks(List<RDAPLink> links)
    {
        return links?
            .Where(x => !string.Equals(x.Rel, "self", StringComparison.OrdinalIgnoreCase))
            .Select(x => Uri.TryCreate(x.Href, UriKind.Absolute, out var uri) &&
                         (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                ? new RDAPLinkItem(string.IsNullOrWhiteSpace(x.Title) ? uri.AbsoluteUri : x.Title, uri.AbsoluteUri)
                : null)
            .Where(x => x != null)
            .DistinctBy(x => x.Url)
            .ToList() ?? [];
    }

    /// <summary>
    ///     Formats a domain name with its Unicode form (if different) and without a trailing dot.
    /// </summary>
    private static string FormatDomain(string ldhName, string unicodeName)
    {
        ldhName = ldhName?.TrimEnd('.');
        unicodeName = unicodeName?.TrimEnd('.');

        if (string.IsNullOrEmpty(unicodeName) ||
            string.Equals(ldhName, unicodeName, StringComparison.OrdinalIgnoreCase))
            return ldhName;

        return string.IsNullOrEmpty(ldhName) ? unicodeName : $"{unicodeName} ({ldhName})";
    }

    /// <summary>
    ///     Formats the result as text for the export.
    /// </summary>
    /// <returns>Result as text.</returns>
    public string ToText()
    {
        var text = new StringBuilder();

        text.AppendLine($"# {Title}");
        text.AppendLine();

        foreach (var detail in Details)
            text.AppendLine($"{detail.Name}: {detail.Value}");

        if (Events.Count > 0)
        {
            text.AppendLine().AppendLine($"## {Strings.Events}");

            foreach (var e in Events)
                text.AppendLine($"{e.Action}: {e.Date}{(string.IsNullOrEmpty(e.Actor) ? "" : $" ({e.Actor})")}");
        }

        if (Nameservers.Count > 0)
        {
            text.AppendLine().AppendLine($"## {Strings.Nameservers}");

            foreach (var ns in Nameservers)
                text.AppendLine(string.IsNullOrEmpty(ns.IPAddresses) ? ns.Name : $"{ns.Name} ({ns.IPAddresses})");
        }

        if (DSData.Count > 0)
        {
            text.AppendLine().AppendLine($"## {Strings.DNSSEC}");

            foreach (var ds in DSData)
                text.AppendLine(
                    $"{Strings.KeyTag}: {ds.KeyTag}, {Strings.Algorithm}: {ds.Algorithm}, {Strings.DigestType}: {ds.DigestType}, {Strings.Digest}: {ds.Digest}");
        }

        if (Contacts.Count > 0)
        {
            text.AppendLine().AppendLine($"## {Strings.Contacts}");

            foreach (var c in Contacts)
            {
                text.AppendLine();
                AppendValue(text, Strings.Roles, c.Roles);
                AppendValue(text, Strings.Handle, c.Handle);
                AppendValue(text, Strings.Name, c.Name);
                AppendValue(text, Strings.Organization, c.Organization);
                AppendValue(text, Strings.Email, c.Email);
                AppendValue(text, Strings.Phone, c.Phone);
                AppendValue(text, Strings.Address, c.Address);
            }
        }

        if (Notices.Count > 0)
        {
            text.AppendLine().AppendLine($"## {Strings.Notices}");

            foreach (var notice in Notices)
            {
                text.AppendLine();

                if (!string.IsNullOrEmpty(notice.Title))
                    text.AppendLine(notice.Title);

                text.AppendLine(notice.Description);

                foreach (var link in notice.Links)
                    text.AppendLine(link.Url);
            }
        }

        return text.ToString();
    }

    private static void AppendValue(StringBuilder text, string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            text.AppendLine($"{name}: {value.Replace(Environment.NewLine, "; ")}");
    }

    #endregion
}

public record RDAPDetailItem(string Name, string Value);

public record RDAPEventItem(string Action, string Date, string Actor);

public record RDAPNameserverItem(string Name, string IPAddresses);

public record RDAPDSDataItem(string KeyTag, string Algorithm, string DigestType, string Digest);

public record RDAPContactItem(
    string Roles,
    string Handle,
    string Name,
    string Organization,
    string Email,
    string Phone,
    string Address);

public record RDAPNoticeItem(string Title, string Description, IReadOnlyList<RDAPLinkItem> Links);

/// <summary>
///     Link of a notice or remark.
/// </summary>
/// <param name="Text">Text to display (title of the link or the URL).</param>
/// <param name="Url">Validated absolute HTTP(S) URL.</param>
public record RDAPLinkItem(string Text, string Url);
