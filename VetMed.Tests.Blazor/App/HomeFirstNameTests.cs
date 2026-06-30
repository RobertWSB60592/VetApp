namespace VetMed.Tests.Blazor.App;

public class HomeFirstNameTests
{
    private static string ResolveFirstName(string? fullName) =>
        fullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Użytkowniku";

    private static string ResolveInitial(string firstName) =>
        firstName.Length > 0 ? firstName[0].ToString() : "?";

    [Theory]
    [InlineData("Jan Kowalski", "Jan")]
    [InlineData("Anna", "Anna")]
    [InlineData("  Marek  Nowak  ", "Marek")]
    public void ResolveFirstName_WhenFullNameHasWords_ReturnsFirstWord(string fullName, string expected)
    {
        var result = ResolveFirstName(fullName);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveFirstName_WhenFullNameIsNullOrWhitespace_ReturnsFallback(string? fullName)
    {
        var result = ResolveFirstName(fullName);

        result.Should().Be("Użytkowniku");
    }

    [Theory]
    [InlineData("Jan Kowalski", 'J')]
    [InlineData("Anna", 'A')]
    [InlineData("  Marek  Nowak  ", 'M')]
    public void ResolveInitial_WhenFirstNameNonEmpty_ReturnsFirstCharacter(string fullName, char expectedInitial)
    {
        var firstName = ResolveFirstName(fullName);
        var initial = ResolveInitial(firstName);

        initial.Should().Be(expectedInitial.ToString());
    }

    [Fact]
    public void ResolveInitial_WhenFirstNameIsFallback_ReturnsFallbackInitial()
    {
        var firstName = ResolveFirstName(null);
        var initial = ResolveInitial(firstName);

        initial.Should().Be("U");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ResolveInitial_WhenInputProducesFallback_DoesNotThrow(string? fullName)
    {
        var firstName = ResolveFirstName(fullName);

        var act = () => ResolveInitial(firstName);

        act.Should().NotThrow<ArgumentOutOfRangeException>();
    }
}
