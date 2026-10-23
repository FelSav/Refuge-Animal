using MuniChien.App.Services;

namespace MuniChien.Tests;

public class LocalClientValidationTests
{
    private static OwnerDirectoryItem ValidOwner() => new()
    {
        FirstName = "Jean",
        LastName = "Exemple",
        Status = "Actif"
    };

    [Fact]
    public void ClientWithoutMunicipalityOrDogIsValid()
    {
        LocalClientValidation.Validate(ValidOwner());
    }

    [Fact]
    public void EmptyFirstNameIsRejected()
    {
        var client = new OwnerDirectoryItem { FirstName = "   ", LastName = "Exemple" };
        Assert.Throws<ArgumentException>(() => LocalClientValidation.Validate(client));
    }

    [Fact]
    public void EmptyLastNameIsRejected()
    {
        var client = new OwnerDirectoryItem { FirstName = "Jean", LastName = "  " };
        Assert.Throws<ArgumentException>(() => LocalClientValidation.Validate(client));
    }

    [Fact]
    public void UnknownMunicipalityIsRejected()
    {
        var client = new OwnerDirectoryItem
        {
            FirstName = "Jean", LastName = "Exemple", Municipality = "Municipalité imaginaire"
        };
        Assert.Throws<ArgumentException>(() => LocalClientValidation.Validate(client));
    }

    [Fact]
    public void KnownMunicipalityIsAccepted()
    {
        var client = new OwnerDirectoryItem
        {
            FirstName = "Jean", LastName = "Exemple", Municipality = "Saint-Félicien"
        };
        LocalClientValidation.Validate(client);
    }

    [Theory]
    [InlineData("@exemple.ca")]
    [InlineData("prenom@")]
    [InlineData("abc")]
    public void InvalidEmailIsRejected(string email)
    {
        var client = new OwnerDirectoryItem
        {
            FirstName = "Jean", LastName = "Exemple", Email = email
        };
        Assert.Throws<ArgumentException>(() => LocalClientValidation.Validate(client));
    }

    [Fact]
    public void UnknownStatusIsRejected()
    {
        var client = new OwnerDirectoryItem
        {
            FirstName = "Jean", LastName = "Exemple", Status = "Suspendu"
        };
        Assert.Throws<ArgumentException>(() => LocalClientValidation.Validate(client));
    }
}
