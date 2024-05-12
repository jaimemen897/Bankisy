using System.ComponentModel.DataAnnotations;
using TFG.Context.DTOs.users;

namespace TFG.ServicesTests.Users;

[TestFixture]
public class UserValidationsTest
{
    private UserCreateDto _user;
    private ValidationContext _context;
    private List<ValidationResult> _results;

    [SetUp]
    public void SetUp()
    {
        _user = new UserCreateDto
        {
            Name = "Test",
            Email = "test@test.com",
            Username = "test",
            Dni = "54522318J",
            Gender = "Male",
            Password = "password",
            Avatar = "avatar.png",
            Phone = "123456789",
        };
        _context = new ValidationContext(_user, null, null);
        _results = [];
    }
    
    [Test]
    public void Name_Null_ThrowsException()
    {
        _user.Name = null!;
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Name field is required."));
        });
    }

    [Test]
    public void Name_TooLong_ThrowsException()
    {
        _user.Name = new string('a', 101);
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Name must be less than 100 characters"));
        });
    }
    
    [Test]
    public void Name_TooShort_ThrowsException()
    {
        _user.Name = "aa";
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Name must be more than 3 characters"));
        });
    }
    
    [Test]
    public void Email_Null_ThrowsException()
    {
        _user.Email = null!;
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Email field is required."));
        });
    }

    [Test]
    public void Email_TooLong_ThrowsException()
    {
        _user.Email = new string('a', 101);
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Email must be less than 100 characters"));
        });
    }
    
    [Test]
    public void Email_TooShort_ThrowsException()
    {
        _user.Email = "aa";
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Email must be more than 3 characters"));
        });
    }

    [Test]
    public void Email_InvalidFormat_ThrowsException()
    {
        _user.Email = "invalid email";
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Email is not valid"));
        });
    }
    
    [Test]
    public void Username_Null_ThrowsException()
    {
        _user.Username = null!;
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Username field is required."));
        });
    }

    [Test]
    public void Username_TooLong_ThrowsException()
    {
        _user.Username = new string('a', 101);
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Username must be less than 100 characters"));
        });
    }
    
    [Test]
    public void Username_TooShort_ThrowsException()
    {
        _user.Username = "aa";
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Username must be more than 3 characters"));
        });
    }
    
    [Test]
    public void Dni_Null_ThrowsException()
    {
        _user.Dni = null!;
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Dni field is required."));
        });
    }
    
    [Test]
    public void Dni_WrongLength_ThrowsException()
    {
        _user.Dni = "1234567890";
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("DNI must be 9 characters"));
        });
    }
    
    [Test]
    public void Password_Null_ThrowsException()
    {
        _user.Password = null!;
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Password field is required."));
        });
    }

    [Test]
    public void Password_TooLong_ThrowsException()
    {
        _user.Password = new string('a', 101);
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Password must be less than 100 characters"));
        });
    }

    [Test]
    public void Avatar_TooLong_ThrowsException()
    {
        _user.Avatar = new string('a', 101);
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Avatar must be less than 100 characters"));
        });
    }
    
    [Test]
    public void Phone_Null_ThrowsException()
    {
        _user.Phone = null!;
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("The Phone field is required."));
        });
    }

    [Test]
    public void Phone_WrongLength_ThrowsException()
    {
        _user.Phone = "1234567890";
        var isValid = Validator.TryValidateObject(_user, _context, _results, true);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(_results[0].ErrorMessage, Is.EqualTo("Phone must be 9 characters"));
        });
    }
}