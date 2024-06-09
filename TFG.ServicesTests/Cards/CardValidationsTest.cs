using System.ComponentModel.DataAnnotations;
using TFG.Context.DTOs.cards;

namespace TFG.ServicesTests.Cards;

[TestFixture]
public class CardValidationsTest
{
    private CardCreateDto _card;
    private ValidationContext _context;
    private List<ValidationResult> _results;

    [SetUp]
    public void SetUp()
    {
        _card = new CardCreateDto
        {
            Pin = "1234",
            CardType = "Debit",
            UserId = Guid.NewGuid(),
            BankAccountIban = "ES12345678901234567890"
        };
        _context = new ValidationContext(_card, null, null);
        _results = new List<ValidationResult>();
    }

    [Test]
    public void Pin_Null_ThrowsException()
    {
        _card.Pin = null!;
        var isValid = Validator.TryValidateObject(_card, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Pin field is required."));
        });
    }

    [Test]
    public void CardType_Null_ThrowsException()
    {
        _card.CardType = null!;
        var isValid = Validator.TryValidateObject(_card, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The CardType field is required."));
        });
    }

    [Test]
    public void UserId_Null_ThrowsException()
    {
        _card.UserId = null;
        var isValid = Validator.TryValidateObject(_card, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The UserId field is required."));
        });
    }

    [Test]
    public void BankAccountIban_Null_ThrowsException()
    {
        _card.BankAccountIban = null!;
        var isValid = Validator.TryValidateObject(_card, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The BankAccountIban field is required."));
        });
    }

    [Test]
    public void BankAccountIban_TooLong_ThrowsException()
    {
        _card.BankAccountIban = new string('a', 25);
        var isValid = Validator.TryValidateObject(_card, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Bank account IBAN must be 24 characters"));
        });
    }
}