using NETworkManager.Models;
using NETworkManager.Models.RunCommand;
using Xunit;

namespace NETworkManager.Tests.RunCommand;

public class RunCommandParserTests
{
    private static readonly RunCommandDefinition[] Commands =
    [
        new()
        {
            Type = RunCommandType.Application, Application = ApplicationName.PingMonitor, Command = "pingmonitor",
            Aliases = ["ping"], SupportsTarget = true, SupportsProfile = true
        },
        new()
        {
            Type = RunCommandType.Application, Application = ApplicationName.RemoteDesktop, Command = "remotedesktop",
            Aliases = ["rdp"], SupportsTarget = true, SupportsProfile = true
        },
        new()
        {
            Type = RunCommandType.Application, Application = ApplicationName.DNSLookup, Command = "dnslookup",
            Aliases = ["dns", "nslookup"], SupportsTarget = true, SupportsProfile = true,
            Argument = RunCommandArgument.DNSServer, Types = ["a", "aaaa", "cname", "ptr"],
            SyntaxParts =
            [
                RunCommandSyntaxPart.Required(RunCommandSyntaxPartKind.Target, "Hosts"),
                RunCommandSyntaxPart.Optional(RunCommandSyntaxPartKind.Argument, "DNS server"),
                RunCommandSyntaxPart.Optional(RunCommandSyntaxPartKind.Type, "Type"),
                RunCommandSyntaxPart.Profile("Profile")
            ]
        },
        new()
        {
            Type = RunCommandType.Application, Application = ApplicationName.PortScanner, Command = "portscanner",
            Aliases = ["portscan"], SupportsTarget = true, SupportsProfile = true, Argument = RunCommandArgument.Ports
        },
        new()
        {
            Type = RunCommandType.Application, Application = ApplicationName.SubnetCalculator,
            Command = "subnetcalculator", Aliases = ["subnet"], SupportsTarget = true,
            Argument = RunCommandArgument.Subnet
        },
        new()
        {
            Type = RunCommandType.Application, Application = ApplicationName.Lookup, Command = "lookup",
            SupportsTarget = true, Types = ["oui", "port"], TypeBeforeTarget = true
        },
        new() { Type = RunCommandType.Application, Application = ApplicationName.Firewall, Command = "firewall" },
        new() { Type = RunCommandType.Setting, Command = "settings" }
    ];

    private static RunCommandParseResult Parse(string input)
    {
        return RunCommandParser.Parse(input, Commands);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Empty(string input)
    {
        var result = Parse(input);

        Assert.Equal(RunCommandParseError.Empty, result.Error);
        Assert.Equal(RunCommandPosition.Command, result.Position);
    }

    [Theory]
    [InlineData("ping", "pingmonitor")]
    [InlineData("PING", "pingmonitor")]
    [InlineData("nslookup", "dnslookup")]
    [InlineData("dnslookup", "dnslookup")]
    public void CommandAndAliases(string input, string command)
    {
        var result = Parse(input);

        Assert.Equal(command, result.Command.Command);
        Assert.Equal(RunCommandParseError.None, result.Error);
        Assert.Equal(RunCommandPosition.Command, result.Position);
        Assert.Equal(input, result.CurrentWord);
    }

    [Fact]
    public void UnknownCommand()
    {
        var result = Parse("pin 10.0.0.1");

        Assert.Null(result.Command);
        Assert.Equal(RunCommandParseError.UnknownCommand, result.Error);
        Assert.Equal("pin", result.ErrorValue);
    }

    [Fact]
    public void CommandWithoutTarget()
    {
        var result = Parse("ping ");

        Assert.Equal(RunCommandParseError.None, result.Error);
        Assert.Null(result.Target);
        Assert.Equal(RunCommandPosition.Target, result.Position);
        Assert.Equal("ping ", result.InputBeforeCurrentWord);
    }

    [Fact]
    public void Target()
    {
        var result = Parse("ping 10.0.0.1");

        Assert.Equal("10.0.0.1", result.Target);
        Assert.Equal(RunCommandPosition.Target, result.Position);
        Assert.Equal("10.0.0.1", result.CurrentWord);
        Assert.Equal("ping ", result.InputBeforeCurrentWord);
    }

    [Theory]
    [InlineData("ping 10.0.0.1; 10.0.0.2")]
    [InlineData("ping 10.0.0.1 ;10.0.0.2")]
    [InlineData("ping 10.0.0.1 ; 10.0.0.2")]
    public void ListWithWhitespace(string input)
    {
        var result = Parse(input);

        Assert.Equal("10.0.0.1;10.0.0.2", result.Target);
        Assert.Equal(RunCommandParseError.None, result.Error);
    }

    [Theory]
    [InlineData("dns borntoberoot.net 1.1.1.1 cname")]
    [InlineData("dns borntoberoot.net CNAME 1.1.1.1")]
    public void DNSServerAndTypeInAnyOrder(string input)
    {
        var result = Parse(input);

        Assert.Equal("borntoberoot.net", result.Target);
        Assert.Equal("1.1.1.1", result.Argument);
        Assert.Equal("cname", result.Type);
        Assert.Equal(RunCommandParseError.None, result.Error);
    }

    // A configured DNS server (by name) after the type, e.g. for a reverse lookup
    [Fact]
    public void DNSServerNameAfterType()
    {
        var result = Parse("dns 1.1.1.1 PTR Cloudflare");

        Assert.Equal("1.1.1.1", result.Target);
        Assert.Equal("ptr", result.Type);
        Assert.Equal("Cloudflare", result.Argument);
        Assert.Equal(RunCommandParseError.None, result.Error);
    }

    [Theory]
    [InlineData("dns borntoberoot.net \"Google Public DNS\" aaaa")]
    [InlineData("dns borntoberoot.net aaaa \"Google Public DNS\"")]
    public void QuotedArgument(string input)
    {
        var result = Parse(input);

        Assert.Equal("Google Public DNS", result.Argument);
        Assert.Equal("aaaa", result.Type);
        Assert.Equal(RunCommandParseError.None, result.Error);
    }

    [Fact]
    public void OpenQuoteIsTheWordThatIsBeingTyped()
    {
        // Whitespace in an open quote doesn't start a new word.
        var result = Parse("dns borntoberoot.net \"Google ");

        Assert.Equal(RunCommandPosition.Argument, result.Position);
        Assert.Equal("Google ", result.CurrentWord);
        Assert.Equal("dns borntoberoot.net ", result.InputBeforeCurrentWord);
    }

    [Fact]
    public void CompletionKeepsQuotes()
    {
        var result = Parse("dns borntoberoot.net \"Google Public DNS\" aa");

        Assert.Equal("aa", result.CurrentWord);
        Assert.Equal("dns borntoberoot.net \"Google Public DNS\" ", result.InputBeforeCurrentWord);
    }

    [Theory]
    [InlineData("rdp @\"Web Server 01\"")]
    [InlineData("rdp \"@Web Server 01\"")]
    public void QuotedProfileName(string input)
    {
        var result = Parse(input);

        Assert.Equal("Web Server 01", result.ProfileName);
    }

    [Fact]
    public void TextAfterAQuotedProfileName()
    {
        var result = Parse("rdp @\"Web Server\" x");

        Assert.Equal("Web Server", result.ProfileName);
        Assert.Equal(RunCommandParseError.UnknownArgument, result.Error);
        Assert.Equal("x", result.ErrorValue);
    }

    [Fact]
    public void ListThatIsBeingTyped()
    {
        // "; " is the separator of a list that is still being typed, not the end of the target.
        var result = Parse("ping 10.0.0.1; ");

        Assert.Equal(RunCommandPosition.Target, result.Position);
        Assert.Equal("10.0.0.1;", result.Target);
        Assert.Equal("ping ", result.InputBeforeCurrentWord);
    }

    [Fact]
    public void SemicolonInQuotesIsKept()
    {
        var result = Parse("dns borntoberoot.net \"DNS ; Server\"");

        Assert.Equal("DNS ; Server", result.Argument);
    }

    [Fact]
    public void HostsAndType()
    {
        var result = Parse("dns 1.1.1.1; 1.0.0.1 ptr");

        Assert.Equal("1.1.1.1;1.0.0.1", result.Target);
        Assert.Equal("ptr", result.Type);
        Assert.Null(result.Argument);
    }

    [Fact]
    public void PortsWithWhitespace()
    {
        var result = Parse("portscan 10.0.0.1; 10.0.0.2 22; 80; 443");

        Assert.Equal("10.0.0.1;10.0.0.2", result.Target);
        Assert.Equal("22;80;443", result.Argument);
        Assert.Equal(RunCommandParseError.None, result.Error);
    }

    [Fact]
    public void TargetThatLooksLikeAType()
    {
        // The second word is always the target.
        var result = Parse("dns a");

        Assert.Equal("a", result.Target);
        Assert.Null(result.Type);
    }

    [Theory]
    [InlineData("ping 10.0.0.1 foo", "foo")]
    [InlineData("dns borntoberoot.net 1.1.1.1 8.8.8.8", "8.8.8.8")]
    [InlineData("dns borntoberoot.net a aaaa 1.1.1.1", "aaaa")]
    public void UnknownArgument(string input, string unknown)
    {
        var result = Parse(input);

        Assert.Equal(RunCommandParseError.UnknownArgument, result.Error);
        Assert.Equal(unknown, result.ErrorValue);
    }

    [Theory]
    [InlineData("firewall 10.0.0.1")]
    [InlineData("settings general")]
    public void NoArguments(string input)
    {
        var result = Parse(input);

        Assert.Equal(RunCommandParseError.NoArguments, result.Error);
    }

    [Fact]
    public void NoArgumentsCurrentWordIsTheLastWord()
    {
        var result = Parse("settings foo ba");

        Assert.Equal("ba", result.CurrentWord);
        Assert.Equal("settings foo ", result.InputBeforeCurrentWord);
    }

    [Theory]
    [InlineData("ping @dns-01", "dns-01")]
    [InlineData("ping @Web Server 01", "Web Server 01")]
    [InlineData("ping   @Web Server 01  ", "Web Server 01")]
    [InlineData("ping @a;b", "a;b")]
    public void Profile(string input, string profileName)
    {
        var result = Parse(input);

        Assert.Equal(profileName, result.ProfileName);
        Assert.Null(result.Target);
        Assert.Equal(RunCommandParseError.None, result.Error);
        Assert.Equal(RunCommandPosition.Target, result.Position);
    }

    [Fact]
    public void ProfileWithoutName()
    {
        var result = Parse("ping @");

        Assert.Equal(string.Empty, result.ProfileName);
        Assert.Equal(RunCommandParseError.MissingProfileName, result.Error);
        Assert.Equal("@", result.CurrentWord);
    }

    [Fact]
    public void ProfileNotSupported()
    {
        var result = Parse("settings @dns-01");

        Assert.Equal(RunCommandParseError.NoProfiles, result.Error);
    }

    [Fact]
    public void HostWithAtSign()
    {
        // "@" is only a profile at the start of the target (e.g. "ssh admin@server01").
        var result = Parse("ping admin@server01");

        Assert.Equal("admin@server01", result.Target);
        Assert.Null(result.ProfileName);
    }

    [Theory]
    [InlineData("dns borntoberoot.net ", RunCommandPosition.Argument, "", "dns borntoberoot.net ")]
    [InlineData("dns borntoberoot.net aa", RunCommandPosition.Argument, "aa", "dns borntoberoot.net ")]
    [InlineData("dns borntoberoot.net 1.1.1.1 cn", RunCommandPosition.Argument, "cn", "dns borntoberoot.net 1.1.1.1 ")]
    [InlineData("dns border", RunCommandPosition.Target, "border", "dns ")]
    public void PositionForSuggestions(string input, RunCommandPosition position, string currentWord, string before)
    {
        var result = Parse(input);

        Assert.Equal(position, result.Position);
        Assert.Equal(currentWord, result.CurrentWord);
        Assert.Equal(before, result.InputBeforeCurrentWord);
    }

    [Theory]
    [InlineData("subnet 10.0.0.0/24", "10.0.0.0/24", null)]
    [InlineData("subnet 10.0.0.0/16 /24", "10.0.0.0/16", "/24")]
    [InlineData("subnet 10.0.0.0/24 10.0.1.0/24", "10.0.0.0/24", "10.0.1.0/24")]
    public void SubnetWithNewSubnetmaskOrSubnet2(string input, string target, string argument)
    {
        var result = Parse(input);

        Assert.Equal(RunCommandParseError.None, result.Error);
        Assert.Equal(target, result.Target);
        Assert.Equal(argument, result.Argument);
    }

    [Theory]
    [InlineData("lookup port 22", "port", "22")]
    [InlineData("lookup PORT 22; 80; ssh", "port", "22;80;ssh")]
    [InlineData("lookup oui 00:11:22", "oui", "00:11:22")]
    [InlineData("lookup oui Cisco Systems", "oui", "Cisco Systems")]
    [InlineData("lookup oui \"Cisco Systems\"", "oui", "Cisco Systems")]
    [InlineData("lookup port", "port", null)]
    public void TypeBeforeTarget(string input, string type, string target)
    {
        var result = Parse(input);

        Assert.Equal(RunCommandParseError.None, result.Error);
        Assert.Equal(type, result.Type);
        Assert.Equal(target, result.Target);
    }

    [Theory]
    [InlineData("lookup po", "po")]
    [InlineData("lookup foo 22", "foo")]
    public void TypeBeforeTargetUnknownType(string input, string unknown)
    {
        var result = Parse(input);

        Assert.Null(result.Type);
        Assert.Equal(RunCommandParseError.UnknownArgument, result.Error);
        Assert.Equal(unknown, result.ErrorValue);
    }

    [Fact]
    public void TypeBeforeTargetNoProfiles()
    {
        var result = Parse("lookup @server");

        Assert.Equal(RunCommandParseError.NoProfiles, result.Error);
    }

    [Theory]
    [InlineData("lookup ", RunCommandPosition.Type, "", "lookup ")]
    [InlineData("lookup po", RunCommandPosition.Type, "po", "lookup ")]
    [InlineData("lookup port ", RunCommandPosition.Target, "", "lookup port ")]
    [InlineData("lookup port 2", RunCommandPosition.Target, "2", "lookup port ")]
    [InlineData("lookup oui Cisco Sys", RunCommandPosition.Target, "Cisco Sys", "lookup oui ")]
    public void TypeBeforeTargetPositionForSuggestions(string input, RunCommandPosition position, string currentWord,
        string before)
    {
        var result = Parse(input);

        Assert.Equal(position, result.Position);
        Assert.Equal(currentWord, result.CurrentWord);
        Assert.Equal(before, result.InputBeforeCurrentWord);
    }

    [Fact]
    public void Syntax()
    {
        var result = Parse("dns ");

        Assert.Equal("<Hosts> [DNS server] [Type] | @<Profile>", result.Command.Syntax);
    }

    [Theory]
    [InlineData("dns", null)]
    [InlineData("dns ", RunCommandSyntaxPartKind.Target)]
    [InlineData("dns border", RunCommandSyntaxPartKind.Target)]
    [InlineData("dns @web", RunCommandSyntaxPartKind.Profile)]
    [InlineData("dns borntoberoot.net ", RunCommandSyntaxPartKind.Argument)]
    [InlineData("dns borntoberoot.net 1.1", RunCommandSyntaxPartKind.Argument)]
    // The DNS server comes before the type, unless a complete type is typed.
    [InlineData("dns borntoberoot.net aa", RunCommandSyntaxPartKind.Argument)]
    [InlineData("dns borntoberoot.net c", RunCommandSyntaxPartKind.Argument)]
    [InlineData("dns borntoberoot.net aaaa", RunCommandSyntaxPartKind.Type)]
    [InlineData("dns borntoberoot.net 1.1.1.1 c", RunCommandSyntaxPartKind.Type)]
    [InlineData("dns borntoberoot.net 1.1.1.1 ", RunCommandSyntaxPartKind.Type)]
    [InlineData("dns borntoberoot.net aaaa ", RunCommandSyntaxPartKind.Argument)]
    [InlineData("dns borntoberoot.net 1.1.1.1 aaaa ", null)]
    [InlineData("lookup ", RunCommandSyntaxPartKind.Type)]
    [InlineData("lookup port 2", RunCommandSyntaxPartKind.Target)]
    [InlineData("portscan 10.0.0.1 ", RunCommandSyntaxPartKind.Argument)]
    public void CurrentSyntaxPart(string input, RunCommandSyntaxPartKind? part)
    {
        Assert.Equal(part, Parse(input).CurrentSyntaxPart);
    }
}
