using System.ComponentModel.DataAnnotations;
using TFG.Context.DTOs.bankAccount;

namespace TFG.ServicesTests.BankAccounts;

[TestFixture]
public class BankAccountValidationsTest
{
    private BankAccountCreateDto _bankAccount;
    private ValidationContext _context;
    private List<ValidationResult> _results;

    [SetUp]
    public void SetUp()
    {
        _bankAccount = new BankAccountCreateDto
        {
            AccountType = "Savings",
            UsersId = new List<Guid> { Guid.NewGuid() },
            AcceptBizum = true,
        };
        _context = new ValidationContext(_bankAccount, null, null);
        _results = [];
    }
    
    [Test]
    public void AccountType_Null_ThrowsException()
    {
        _bankAccount.AccountType = null!;
        var isValid = Validator.TryValidateObject(_bankAccount, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The AccountType field is required."));
        });
    }

    [Test]
    public void UsersId_Empty_ThrowsException()
    {
        _bankAccount.UsersId = null!;
        var isValid = Validator.TryValidateObject(_bankAccount, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The UsersId field is required."));
        });
    }
}