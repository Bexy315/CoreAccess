using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.Repositories;

public interface IUserRepository
{
    public Task<List<CoreUser>> SearchUsersAsync(CoreUserSearchOptions options);
    public Task<CoreUser> InsertOrUpdateUserAsync(CoreUser user);
    public Task DeleteUserAsync(string id);
}
public class UserRepository(CoreAccessDbContext context) : IUserRepository
{
    public async Task<List<CoreUser>> SearchUsersAsync(CoreUserSearchOptions options)
    {
        var users = await context.Users.ToListAsync();
            
        var query = users.AsQueryable();

        if (!string.IsNullOrEmpty(options.Search))
        {
            query = query.Where(u => u.Username.ToLower().Contains(options.Search.ToLower()) ||
                                     u.Email.ToLower().Contains(options.Search.ToLower()) ||
                                     u.FirstName.ToLower().Contains(options.Search.ToLower()) ||
                                     u.LastName.ToLower().Contains(options.Search.ToLower()));
        }

        if (!string.IsNullOrEmpty(options.Id))
        {
            var idLower = options.Id.ToLower();
            query = query.Where(r => r.Id.ToString().ToLower() == idLower);
        }

        if (!string.IsNullOrEmpty(options.Name))
        {
            query = query.Where(u => u.FirstName.ToLower().Contains(options.Name.ToLower()) || u.LastName.ToLower().Contains(options.Name.ToLower()));
        }

        if (!string.IsNullOrEmpty(options.Email))
        {
            query = query.Where(u => u.Email.Contains(options.Email));
        }

        if (!string.IsNullOrEmpty(options.Phone))
        {
            query = query.Where(u => u.Phone.Contains(options.Phone));
        }

        if (!string.IsNullOrEmpty(options.Address))
        {
            query = query.Where(u => u.Address.Contains(options.Address));
        }

        if (!string.IsNullOrEmpty(options.City))
        {
            query = query.Where(u => u.City.ToLower().Contains(options.City.ToLower()));
        }

        if (!string.IsNullOrEmpty(options.State))
        {
            query = query.Where(u => u.State.ToLower().Contains(options.State.ToLower()));
        }

        if (!string.IsNullOrEmpty(options.Zip))
        {
            query = query.Where(u => u.Zip.Contains(options.Zip));
        }

        if (!string.IsNullOrEmpty(options.Country))
        {
            query = query.Where(u => u.Country.ToLower().Contains(options.Country.ToLower()));
        }

        if (options.Status.HasValue)
        {
            query = query.Where(u => u.Status == options.Status.Value);
        }

        var skip = (options.Page - 1) * options.PageSize;
        var result = query.Skip(skip).Take(options.PageSize).ToList();

        return result;
    }
    public async Task<CoreUser> InsertOrUpdateUserAsync(CoreUser user)
    {
        var existingUser = await context.Users.FindAsync(user.Id);
        if (existingUser != null)
        {
            context.Entry(existingUser).CurrentValues.SetValues(user);
        }
        else
        {
            await context.Users.AddAsync(user);
        }
        await context.SaveChangesAsync();
        
        return await context.Users.FindAsync(user.Id);
    }

    public async Task DeleteUserAsync(string id)
    {
        var user = await context.Users.FindAsync(Guid.Parse(id));
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("User not found");
        }
    }
}