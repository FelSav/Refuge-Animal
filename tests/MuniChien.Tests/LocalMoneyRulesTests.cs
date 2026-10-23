using MuniChien.App.Services;

namespace MuniChien.Tests;

public class LocalMoneyRulesTests
{
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("100000.01")]
    [InlineData("1.999")]
    [InlineData("25.000")]
    public void InvalidAmountsAreRejected(string input)
    {
        decimal amount = decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Throws<ArgumentException>(() => LocalMoneyRules.ValidatePositiveAmount(amount));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("25.50")]
    [InlineData("100000.00")]
    public void ValidAmountsAreAccepted(string input)
    {
        decimal amount = decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
        LocalMoneyRules.ValidatePositiveAmount(amount);
    }

    [Fact]
    public void FuturePaymentDateIsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            LocalMoneyRules.ValidatePaymentDate(DateTime.Today.AddDays(1)));
    }

    [Fact]
    public void OldPaymentDateIsRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            LocalMoneyRules.ValidatePaymentDate(new DateTime(1999, 12, 31)));
    }

    [Fact]
    public void TodayIsAcceptedAsPaymentDate()
    {
        LocalMoneyRules.ValidatePaymentDate(DateTime.Today);
    }
}
