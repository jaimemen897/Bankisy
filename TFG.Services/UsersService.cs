using TFG.Context.Context;
using TFG.Context.Models;

namespace TFG.Services;

public class UsersService
{
    private readonly BankContext _bankContext;

    public UsersService(BankContext bankContext)
    {
        _bankContext = bankContext;
    }

    public List<User> GetUsers()
    {
        return _bankContext.Users.ToList();
    }

    public User? GetUser(Guid id)
    {
        return _bankContext.Users.Find(id);
    }

    public User CreateUser(User user)
    {
        user.Id = Guid.NewGuid();

        user.CreatedAt = DateTime.Now.ToUniversalTime();
        user.UpdatedAt = DateTime.Now.ToUniversalTime();
        _bankContext.Users.Add(user);
        _bankContext.SaveChanges();

        return user;
    }

    public User? UpdateUser(Guid id, User user)
    {
        var userToUpdate = _bankContext.Users.Find(id);
        if (userToUpdate == null)
        {
            return null;
        }
        
        userToUpdate.Name = user.Name;
        userToUpdate.Email = user.Email;
        userToUpdate.UpdatedAt = DateTime.Now.ToUniversalTime();
        _bankContext.SaveChanges();
        
        return userToUpdate;
    }

    public bool DeleteUser(Guid id)
    {
        var user = _bankContext.Users.Find(id);
        if (user == null)
        {
            return false;
        }

        _bankContext.Users.Remove(user);
        _bankContext.SaveChanges();

        return true;
    }
}