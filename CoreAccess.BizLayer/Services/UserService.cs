using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using CoreAccess.Models.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CoreAccess.BizLayer.Services;

public interface IUserService
{
    Task<PagedResult<UserDto>> SearchUsersAsync(UserSearchOptions options, CancellationToken cancellationToken = default);
    Task<UserDetailDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserDetailDto> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserDetailDto> CreateUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
    Task<UserDetailDto> UpdateUserAsync(string userId, UserUpdateRequest user, CancellationToken cancellationToken = default);
    Task<UserDetailDto> UpdateUserProfilePicutre(string userId, IFormFile profilePicture, CancellationToken cancellationToken = default);
    Task<UserDetailDto> AddRoleToUserAsync(string userId, string roleId, CancellationToken cancellationToken = default);
    Task<UserDetailDto> RemoveRoleFromUserAsync(string userId, string roleId, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> ValidateCredentialsByUsernameAsync(string username, string password, CancellationToken cancellationToken = default);
}
public class UserService(IUserRepository userRepository, IRoleRepository roleRepository, ILogger<IUserService> logger) : IUserService
{
    public async Task<PagedResult<UserDto>> SearchUsersAsync(UserSearchOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await userRepository.SearchUsersAsync(options, cancellationToken);
            var dto = new PagedResult<UserDto>
            {
                Items = result.Items.Select(x => x.ToDto()).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            return dto;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<UserDetailDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
        }

        try
        {
            var user = await userRepository.SearchUsersAsync(new UserSearchOptions
            {
                Id = userId,
                IncludeRoles = true,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
            if (user == null)
            {
                throw new KeyNotFoundException("User not found for the provided ID.");
            }

            return user.ToDetailDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<UserDetailDto> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentNullException(nameof(username), "Username cannot be null or empty");
        }

        try
        {
            var user = await userRepository.SearchUsersAsync(new UserSearchOptions
            {
                Username = username,
                IncludeRoles = true,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
            if (user == null)
            {
                throw new KeyNotFoundException("User not found for the provided username.");
            }

            return user.ToDetailDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<UserDetailDto> CreateUserAsync(UserCreateRequest user, CancellationToken cancellationToken = default)
    {
        try
        {
            var newUser = new User
            {
                Username = user.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Address = user.Address,
                City = user.City,
                State = user.State,
                Zip = user.Zip,
                Country = user.Country,
                Status = user.Status,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            
            var createdUser = await userRepository.InsertOrUpdateUserAsync(newUser, cancellationToken);
            if (createdUser == null)
            {
                throw new Exception("Failed to create user");
            }
            return createdUser.ToDetailDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<UserDetailDto> UpdateUserAsync(string userId, UserUpdateRequest user, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await userRepository.SearchUsersAsync(new UserSearchOptions()
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
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

            var updatedUser = await userRepository.InsertOrUpdateUserAsync(existingUser, cancellationToken);
            
            if (updatedUser == null)
            {
                throw new Exception("Failed to update user. Updated user not found");
            }
            
            return updatedUser.ToDetailDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<UserDetailDto> UpdateUserProfilePicutre(string userId, IFormFile profilePicture, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
        }

        if (profilePicture == null || profilePicture.Length == 0)
        {
            throw new ArgumentException("Profile picture cannot be null or empty", nameof(profilePicture));
        }

        try
        {
            var user = await userRepository.SearchUsersAsync(new UserSearchOptions()
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
        
            using var memoryStream = new MemoryStream();
            await profilePicture.CopyToAsync(memoryStream, cancellationToken);
            user.ProfilePicture = memoryStream.ToArray();
            user.ProfilePictureContentType = profilePicture.ContentType;
            user.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var updatedUser = await userRepository.InsertOrUpdateUserAsync(user, cancellationToken);

            return updatedUser.ToDetailDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<UserDetailDto> AddRoleToUserAsync(string userId, string roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userRepository.SearchUsersAsync(new UserSearchOptions()
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
            if (user == null)
            {
                throw new KeyNotFoundException("User not found for the provided ID.");
            }
            
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions{
                Id = roleId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
            if (role == null)
            {
                throw new KeyNotFoundException("Role not found for the provided ID.");
            }

            user.Roles.Add(role);
            var updatedUser = await userRepository.InsertOrUpdateUserAsync(user, cancellationToken);

            return updatedUser.ToDetailDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<UserDetailDto> RemoveRoleFromUserAsync(string userId, string roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userRepository.SearchUsersAsync(new UserSearchOptions()
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
            if (user == null)
            {
                throw new KeyNotFoundException("User not found for the provided ID.");
            }
            
            var role = await roleRepository.SearchRolesAsync(new RoleSearchOptions{
                Id = roleId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
            if (role == null)
            {
                throw new KeyNotFoundException("Role not found for the provided ID.");
            }

            user.Roles.RemoveAll(r => r.Id == role.Id);
            var updatedUser = await userRepository.InsertOrUpdateUserAsync(user, cancellationToken);

            return updatedUser.ToDetailDto();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task DeleteUserAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await userRepository.DeleteUserAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUsers = await userRepository.SearchUsersAsync(new UserSearchOptions
            {
                Username = username,
                Page = 1,
                PageSize = 1
            }, cancellationToken);
            return existingUsers.Items.Any();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> ValidateCredentialsByUsernameAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if(username == null || password == null)
        {
            throw new ArgumentException("Username and Password must be provided.");
        }
        
        try
        {
            var user = await userRepository.SearchUsersAsync(new UserSearchOptions
            {
                Username = username,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.Items.FirstOrDefault() ?? null, cancellationToken: cancellationToken);

            if (user == null || user.Status != UserStatus.Active || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating credentials for user {Username}", username);
            return false;
        }
    }
}