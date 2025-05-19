using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;

namespace CoreAccess.WebAPI.Services;

public interface IUserService
{
    Task<PagedResult<CoreUserDto>> SearchUsersAsync(CoreUserSearchOptions options);
    Task<CoreUserDto> CreateUserAsync(CoreUserCreateRequest request);
    Task<CoreUserDto> UpdateUserAsync(string userId, CoreUserUpdateRequest user);
    Task DeleteUserAsync(string id);
}
internal class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<PagedResult<CoreUserDto>> SearchUsersAsync(CoreUserSearchOptions options)
    {
        try
        {
            var result = await userRepository.SearchUsersAsync(options);
            var dto = new PagedResult<CoreUserDto>
            {
                Items = result.Select(x => new CoreUserDto(x)).ToList(),
                TotalCount = result.Count,
                Page = options.Page,
                PageSize = options.PageSize
            };
            return dto;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreUserDto> CreateUserAsync(CoreUserCreateRequest user)
    {
        try
        {
            var newUser = new CoreUser
            {
                Username = user.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            
            var createdUser = await userRepository.InsertOrUpdateUserAsync(newUser);
            if (createdUser == null)
            {
                throw new Exception("Failed to create user");
            }
            return new CoreUserDto(createdUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreUserDto> UpdateUserAsync(string userId, CoreUserUpdateRequest user)
    {
        try
        {
            var existingUser = await userRepository.SearchUsersAsync(new CoreUserSearchOptions()
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }).ContinueWith(t => t.Result.FirstOrDefault() ?? null);
            
            if (existingUser == null)
            {
                throw new Exception("User not found");
            }

            existingUser.Username = user.Username ?? existingUser.Username;
            existingUser.Email = user.Email ?? existingUser.Email;
            existingUser.FirstName = user.FirstName ?? existingUser.FirstName;
            existingUser.LastName = user.LastName ?? existingUser.LastName;
            existingUser.Phone = user.Phone ?? existingUser.Phone;
            existingUser.Address = user.Address ?? existingUser.Address;
            existingUser.City = user.City ?? existingUser.City;
            existingUser.State = user.State ?? existingUser.State;
            existingUser.Zip = user.Zip ?? existingUser.Zip;
            existingUser.Country = user.Country ?? existingUser.Country;
            existingUser.Status = user.Status ?? existingUser.Status;
            existingUser.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var updatedUser = await userRepository.InsertOrUpdateUserAsync(existingUser);
            if (updatedUser == null)
            {
                throw new Exception("Failed to update user. Updated user not found");
            }
            return new CoreUserDto(updatedUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task DeleteUserAsync(string id)
    {
        try
        {
            await userRepository.DeleteUserAsync(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}