using System.ComponentModel.DataAnnotations;
using TFG.Context.DTOs.transactions;

namespace TFG.ServicesTests.Transactions;

[TestFixture]
public class TransactionValidationsTest
{
    private TransactionCreateDto _transaction;
    private ValidationContext _context;
    private List<ValidationResult> _results;

    [SetUp]
    public void SetUp()
    {
        _transaction = new TransactionCreateDto
        {
            Concept = "Test Transaction",
            Amount = 100.00m,
            IbanAccountOrigin = "ES12345678901234567890",
            IbanAccountDestination = "ES09876543210987654321"
        };
        _context = new ValidationContext(_transaction, null, null);
        _results = new List<ValidationResult>();
    }

    [Test]
    public void Concept_Null_ThrowsException()
    {
        _transaction.Concept = null!;
        var isValid = Validator.TryValidateObject(_transaction, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Concept field is required."));
        });
    }

    [Test]
    public void Concept_TooLong_ThrowsException()
    {
        _transaction.Concept = new string('a', 256);
        var isValid = Validator.TryValidateObject(_transaction, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The concept must be less than 255 characters"));
        });
    }

    [Test]
    public void Amount_LessThanMinimum_ThrowsException()
    {
        _transaction.Amount = 0.00m;
        var isValid = Validator.TryValidateObject(_transaction, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The amount must be greater than 0"));
        });
    }

    [Test]
    public void IbanAccountOrigin_Null_ThrowsException()
    {
        _transaction.IbanAccountOrigin = null!;
        var isValid = Validator.TryValidateObject(_transaction, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The IbanAccountOrigin field is required."));
        });
    }

    [Test]
    public void IbanAccountOrigin_TooShort_ThrowsException()
    {
        _transaction.IbanAccountOrigin = new string('a', 23);
        var isValid = Validator.TryValidateObject(_transaction, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Iban account origin must be 24 characters long"));
        });
    }

    [Test]
    public void IbanAccountOrigin_TooLong_ThrowsException()
    {
        _transaction.IbanAccountOrigin = new string('a', 25);
        var isValid = Validator.TryValidateObject(_transaction, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Iban account origin must be 24 characters long"));
        });
    }

    [Test]
    public void IbanAccountDestination_Null_ThrowsException()
    {
        _transaction.IbanAccountDestination = null!;
        var isValid = Validator.TryValidateObject(_transaction, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Iban account origin must be 24 characters long"));
        });
    }

    [Test]
    public void IbanAccountDestination_TooShort_ThrowsException()
    {
        _transaction.IbanAccountDestination = new string('a', 23);
        var isValid = Validator.TryValidateObject(_transaction, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Iban account origin must be 24 characters long"));
        });
    }

    [Test]
    public void IbanAccountDestination_TooLong_ThrowsException()
    {
        _transaction.IbanAccountDestination = new string('a', 25);
        var isValid = Validator.TryValidateObject(_transaction, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Iban account origin must be 24 characters long"));
        });
    }
}