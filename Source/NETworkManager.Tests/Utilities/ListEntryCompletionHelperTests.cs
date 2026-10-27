using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class ListEntryCompletionHelperTests
{
    private static readonly string[] Names = ["Webserver", "Webserver (Other)", "Database", "DNS (via TCP)", "Mailserver"];

    [Theory]
    [InlineData("", "", "")]
    [InlineData("dat", "", "dat")]
    [InlineData("webserver; dat", "webserver; ", "dat")]
    [InlineData("webserver;dat", "webserver;", "dat")]
    [InlineData("webserver; ", "webserver; ", "")]
    [InlineData("22; 80; Mail", "22; 80; ", "Mail")]
    public void SplitLastEntry(string text, string expectedPrefix, string expectedEntry)
    {
        var (prefix, entry) = ListEntryCompletionHelper.SplitLastEntry(text);

        Assert.Equal(expectedPrefix, prefix);
        Assert.Equal(expectedEntry, entry);
    }

    [Fact]
    public void GetCandidates_StartsWithBeforeContains()
    {
        // No name starts with "server", all three contain it
        Assert.Equal(["Mailserver", "Webserver", "Webserver (Other)"],
            ListEntryCompletionHelper.GetCandidates("server", Names));

        Assert.Equal(["Webserver", "Webserver (Other)"], ListEntryCompletionHelper.GetCandidates("web", Names));
        Assert.Equal(["Database", "DNS (via TCP)"], ListEntryCompletionHelper.GetCandidates("d", Names));

        // Case-insensitive, also in the middle of a name
        Assert.Equal(["DNS (via TCP)"], ListEntryCompletionHelper.GetCandidates("tcp", Names));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("xyz")]
    public void GetCandidates_None(string entry)
    {
        Assert.Empty(ListEntryCompletionHelper.GetCandidates(entry, Names));
    }

    [Fact]
    public void GetCandidates_DuplicateNamesOnce()
    {
        Assert.Equal(["Webserver"], ListEntryCompletionHelper.GetCandidates("web", ["Webserver", "webserver"]));
    }
}
