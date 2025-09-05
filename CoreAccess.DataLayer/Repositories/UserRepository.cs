using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreAccess.DataLayer.DbContext;
using CoreAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.DataLayer.Repositories;

public interface IUserRepository
{
    public Task<List<User>> SearchUsersAsync(UserSearchOptions options, CancellationToken cancellationToken = default);
    public Task<User> InsertOrUpdateUserAsync(User user, CancellationToken cancellationToken = default);
    public Task DeleteUserAsync(string id, CancellationToken cancellationToken = default);
}
public class UserRepository(CoreAccessDbContext context) : IUserRepository
{
    public async Task<List<User>> SearchUsersAsync(UserSearchOptions options, CancellationToken cancellationToken = default)
    {
        var users = await context.Users.Include(u => u.Roles).ToListAsync(cancellationToken);
            
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
        
        if (!string.IsNullOrEmpty(options.Username))
        {
            query = query.Where(u => u.Username.ToLower().Contains(options.Username.ToLower()));
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
    public async Task<User> InsertOrUpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var existingUser = await context.Set<User>().FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        
        if (existingUser == null)
        {
            var newUser = await context.Set<User>().AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            return newUser.Entity;
        }

        context.Entry(existingUser).CurrentValues.SetValues(user);
        context.Entry(existingUser).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
        
        existingUser = await context.Set<User>().FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        
        return existingUser;
    }
    public async Task DeleteUserAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync(Guid.Parse(id), cancellationToken);
        
        
        if (user != null)
        {
            if (user.IsSystem)
                throw new InvalidOperationException("Systembenutzer dürfen nicht gelöscht werden.");
            
            context.Users.Remove(user);
            context.Entry(user).State = EntityState.Deleted;
            
            await context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new Exception("User not found");
        }
    }
}