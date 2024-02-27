using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
    
    public User GetUser(Guid id)
    {
        var user = _bankContext.Users.Find(id);
        
    }
}