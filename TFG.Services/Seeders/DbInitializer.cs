using TFG.Context.Context;
using TFG.Context.Models;

namespace TFG.Services.Seeders;

public static class DbInitializer
{
    public static void SeedData(BankContext context)
    {
        if (context.BankAccounts.Any())
        {
            return;
        }
        context.Users.Add(new User { Id = new Guid(), Name = "Admin", Email = "admin@admin.com", Username="admin", Dni = "54532318J", Gender = Gender.Male, Password = "admin", Role = Roles.Admin, IsDeleted = false });
        context.SaveChanges();
    }
}