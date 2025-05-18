using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.Repositories;

public interface IUserRepository
{
    public Task<List<CoreUser>> SearchUsersAsync(CoreUserSearchOptions options);
    
}
public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<List<CoreUser>> SearchUsersAsync(CoreUserSearchOptions options)
    {
        var users = await context.Users.ToListAsync();
            
        var query = users.AsQueryable();

        if (!string.IsNullOrEmpty(options.Search))
        {
            query = query.Where(u => u.Username.Contains(options.Search) ||
                                     u.Email.Contains(options.Search) ||
                                     u.FirstName.Contains(options.Search) ||
                                     u.LastName.Contains(options.Search));
        }

        if (!string.IsNullOrEmpty(options.Id))
        {
            query = query.Where(u => u.Id.ToString() == options.Id);
        }

        if (!string.IsNullOrEmpty(options.Name))
        {
            query = query.Where(u => u.FirstName.Contains(options.Name) || u.LastName.Contains(options.Name));
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
            query = query.Where(u => u.City.Contains(options.City));
        }

        if (!string.IsNullOrEmpty(options.State))
        {
            query = query.Where(u => u.State.Contains(options.State));
        }

        if (!string.IsNullOrEmpty(options.Zip))
        {
            query = query.Where(u => u.Zip.Contains(options.Zip));
        }

        if (!string.IsNullOrEmpty(options.Country))
        {
            query = query.Where(u => u.Country.Contains(options.Country));
        }

        if (options.Status.HasValue)
        {
            query = query.Where(u => u.Status == options.Status.Value);
        }

        var skip = (options.Page - 1) * options.PageSize;
        var result = query.Skip(skip).Take(options.PageSize).ToList();

        return result;
    }
}