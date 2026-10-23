using MuniChien.App.Services;

namespace MuniChien.Tests;

public class LocalClientSearchRulesTests
{
    [Theory]
    [InlineData("Tremblay")]
    [InlineData("jean")]
    [InlineData("JEAN TREMBLAY")]
    public void MatchesClientNameIgnoringCase(string query)
    {
        Assert.True(LocalClientSearchRules.Matches(query, "Jean Tremblay", "(418) 123-4567", "", "12345"));
    }

    [Fact]
    public void MatchesNameWithoutAccents()
    {
        Assert.True(LocalClientSearchRules.Matches("Felix", "Félix Savard", "", "", "12345"));
    }

    [Theory]
    [InlineData("4181234567")]
    [InlineData("123-4567")]
    [InlineData("4567")]
    [InlineData("(418) 123")]
    public void MatchesPhoneRegardlessOfFormatting(string query)
    {
        Assert.True(LocalClientSearchRules.Matches(query, "Jean Tremblay", "(418) 123-4567", "", "12345"));
    }

    [Fact]
    public void MatchesCellPhoneAndFileNumber()
    {
        Assert.True(LocalClientSearchRules.Matches("5559", "Marie Simard", "", "418-555-9000", "12345"));
        Assert.True(LocalClientSearchRules.Matches("2345", "Marie Simard", "", "", "12345"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("j")]
    [InlineData("non-existant")]
    public void ShortOrUnknownQueriesDoNotMatch(string query)
    {
        Assert.False(LocalClientSearchRules.Matches(query, "Jean Tremblay", "418-123-4567", "", "12345"));
    }
}
