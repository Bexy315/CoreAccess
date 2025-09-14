using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreAccess.BizLayer.Services;
using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoreAccess.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<ILogger<IUserService>> _loggerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _loggerMock = new Mock<ILogger<IUserService>>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUserDetailDto_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Username = "testuser", PasswordHash = "hash", Status = UserStatus.Active };
            _userRepositoryMock
                .Setup(r => r.SearchUsersAsync(It.Is<UserSearchOptions>(o => o.Id == userId.ToString()), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { user });

            // Act
            var result = await _userService.GetUserByIdAsync(userId.ToString());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task GetUserByIdAsync_Throws_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRepositoryMock
                .Setup(r => r.SearchUsersAsync(It.IsAny<UserSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User>());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetUserByIdAsync(userId.ToString()));
        }

        [Fact]
        public async Task CreateUserAsync_CreatesUser_WhenValidRequest()
        {
            // Arrange
            var request = new UserCreateRequest
            {
                Username = "newuser",
                Password = "secret",
                Email = "test@test.com"
            };
            var createdUser = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = request.Email,
                Status = UserStatus.Active
            };
            _userRepositoryMock
                .Setup(r => r.InsertOrUpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _userService.CreateUserAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Username, result.Username);
            Assert.Equal(createdUser.Id, result.Id);
        }

        [Fact]
        public async Task UsernameExistsAsync_ReturnsTrue_WhenUserExists()
        {
            // Arrange
            var username = "exists";
            var users = new List<User> { new User { Id = Guid.NewGuid(), Username = username } };
            _userRepositoryMock
                .Setup(r => r.SearchUsersAsync(It.Is<UserSearchOptions>(o => o.Username == username), It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            // Act
            var result = await _userService.UsernameExistsAsync(username);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UsernameExistsAsync_ReturnsFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var username = "doesnotexist";
            _userRepositoryMock
                .Setup(r => r.SearchUsersAsync(It.Is<UserSearchOptions>(o => o.Username == username), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User>());

            // Act
            var result = await _userService.UsernameExistsAsync(username);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateCredentialsByUsernameAsync_ReturnsTrue_WhenValidCredentials()
        {
            // Arrange
            var username = "validuser";
            var password = "password123";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User { Id = Guid.NewGuid(), Username = username, PasswordHash = hash, Status = UserStatus.Active };
            _userRepositoryMock
                .Setup(r => r.SearchUsersAsync(It.Is<UserSearchOptions>(o => o.Username == username), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { user });

            // Act
            var result = await _userService.ValidateCredentialsByUsernameAsync(username, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateCredentialsByUsernameAsync_ReturnsFalse_WhenPasswordInvalid()
        {
            // Arrange
            var username = "validuser";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("otherpass"),
                Status = UserStatus.Active
            };
            _userRepositoryMock
                .Setup(r => r.SearchUsersAsync(It.Is<UserSearchOptions>(o => o.Username == username), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { user });

            // Act
            var result = await _userService.ValidateCredentialsByUsernameAsync(username, "wrongpassword");

            // Assert
            Assert.False(result);
        }
    }
}
