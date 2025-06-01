using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Repositories;

namespace CoreAccess.WebAPI.Services;

public interface IUserService
{
    Task<PagedResult<CoreUserDto>> SearchUsersAsync(CoreUserSearchOptions options, CancellationToken cancellationToken = default);
    Task<CoreUser> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<CoreUser> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<CoreUser> GetCoreUserByDto(CoreUserDto dto, CancellationToken cancellationToken = default);
    Task<CoreUser> CreateUserAsync(CoreUserCreateRequest request, CancellationToken cancellationToken = default);
    Task<CoreUser> UpdateUserAsync(string userId, CoreUserUpdateRequest user, CancellationToken cancellationToken = default);
    Task<CoreUser> UpdateUserProfilePicutre(string userId, IFormFile profilePicture, CancellationToken cancellationToken = default);
    Task<CoreUser> AddRoleToUserAsync(string userName, string roleId, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<CoreUser> ValidateCredentialsByUsernameAsync(string username, string password, CancellationToken cancellationToken = default);
}
internal class UserService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IRoleRepository roleRepository, IRoleService roleService) : IUserService
{
    public async Task<PagedResult<CoreUserDto>> SearchUsersAsync(CoreUserSearchOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await userRepository.SearchUsersAsync(options, cancellationToken);
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

    public async Task<CoreUser> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
        }

        try
        {
            var user = await userRepository.SearchUsersAsync(new CoreUserSearchOptions
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
            if (user == null)
            {
                throw new KeyNotFoundException("User not found for the provided ID.");
            }

            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreUser> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentNullException(nameof(refreshToken), "Refresh token cannot be null or empty");
        }

        try
        {
            var userId = await refreshTokenRepository.GetUserIdByRefreshToken(refreshToken, cancellationToken);

            var user = await userRepository.SearchUsersAsync(new CoreUserSearchOptions
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            if(user == null)
            {
                throw new KeyNotFoundException("User not found for the provided refresh token.");
            }

            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreUser> GetCoreUserByDto(CoreUserDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "CoreUserDto cannot be null");
        }

        try
        {
            var user = await userRepository.SearchUsersAsync(new CoreUserSearchOptions
            {
                Id = dto.Id.ToString(),
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
            if (user == null)
            {
                throw new KeyNotFoundException("User not found for the provided CoreUserDto.");
            }

            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreUser> CreateUserAsync(CoreUserCreateRequest user, CancellationToken cancellationToken = default)
    {
        try
        {
            var newUser = new CoreUser
            {
                Username = user.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            };
            
            var createdUser = await userRepository.InsertOrUpdateUserAsync(newUser, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);
            if (createdUser == null)
            {
                throw new Exception("Failed to create user");
            }
            return createdUser;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreUser> UpdateUserAsync(string userId, CoreUserUpdateRequest user, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await userRepository.SearchUsersAsync(new CoreUserSearchOptions()
            {
                Id = userId,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
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
            await userRepository.SaveChangesAsync(cancellationToken);
            
            if (updatedUser == null)
            {
                throw new Exception("Failed to update user. Updated user not found");
            }
            
            return updatedUser;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreUser> UpdateUserProfilePicutre(string userId, IFormFile profilePicture, CancellationToken cancellationToken = default)
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
            var user = await GetUserByIdAsync(userId, cancellationToken);
            using var memoryStream = new MemoryStream();
            await profilePicture.CopyToAsync(memoryStream, cancellationToken);
            user.ProfilePicture = memoryStream.ToArray();
            user.ProfilePictureContentType = profilePicture.ContentType;
            user.UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var updatedUser = await userRepository.InsertOrUpdateUserAsync(user, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);

            return updatedUser;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<CoreUser> AddRoleToUserAsync(string userId, string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
        {
            throw new ArgumentException("User ID and Role Name must be provided.");
        }

        try
        {
            var user = await GetUserByIdAsync(userId, cancellationToken);
            var role = await roleRepository.SearchRolesAsync(new CoreRoleSearchOptions{
                Name = roleName,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken: cancellationToken);
            
            if (user == null || role == null)
            {
                throw new KeyNotFoundException("User or Role not found for the provided IDs.");
            }

            user.Roles.Add(role);
            var updatedUser = await userRepository.InsertOrUpdateUserAsync(user, cancellationToken);
            await userRepository.SaveChangesAsync(cancellationToken);

            return updatedUser;
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
            var existingUsers = await userRepository.SearchUsersAsync(new CoreUserSearchOptions
            {
                Username = username,
                Page = 1,
                PageSize = 1
            }, cancellationToken);
            return existingUsers.Any();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<CoreUser> ValidateCredentialsByUsernameAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if(username == null || password == null)
        {
            throw new ArgumentException("Username and Password must be provided.");
        }
        
        try
        {
            var user = await userRepository.SearchUsersAsync(new CoreUserSearchOptions
            {
                Username = username,
                Page = 1,
                PageSize = 1
            }, cancellationToken).ContinueWith(t => t.Result.FirstOrDefault() ?? null, cancellationToken: cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}