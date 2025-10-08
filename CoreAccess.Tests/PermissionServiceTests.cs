using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreAccess.BizLayer.Services;
using CoreAccess.DataLayer.Repositories;
using CoreAccess.Models;
using Moq;
using Xunit;

namespace CoreAccess.Tests
{
    public class PermissionServiceTests
    {
        private readonly Mock<IPermissionRepository> _repoMock;
        private readonly PermissionService _service;

        public PermissionServiceTests()
        {
            _repoMock = new Mock<IPermissionRepository>();
            _service = new PermissionService(_repoMock.Object);
        }

        [Fact]
        public async Task SearchPermissionsAsync_ReturnsPagedResult()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                new Permission { Id = "1", Name = "Read" },
                new Permission { Id = "2", Name = "Write" }
            };
            _repoMock
                .Setup(r => r.SearchPermissionsAsync(It.IsAny<PermissionSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Permission> { Items = permissions, TotalCount = permissions.Count });

            // Act
            var result = await _service.SearchPermissionsAsync(new PermissionSearchOptions { Page = 1, PageSize = 10 });

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count());
            Assert.Contains(result.Items, i => i.Name == "Read");
        }

        [Fact]
        public async Task GetPermissionByIdAsync_ReturnsDetailDto()
        {
            // Arrange
            var permission = new Permission { Id = "123", Name = "Admin" };
            _repoMock
                .Setup(r => r.SearchPermissionsAsync(It.Is<PermissionSearchOptions>(o => o.Id == "123"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Permission> { Items = new List<Permission> { permission }, TotalCount = 1 });

            // Act
            var result = await _service.GetPermissionByIdAsync("123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Admin", result.Name);
        }

        [Fact]
        public async Task GetPermissionByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _repoMock
                .Setup(r => r.SearchPermissionsAsync(It.IsAny<PermissionSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Permission> { Items = new List<Permission>(), TotalCount = 0 });

            // Act
            var result = await _service.GetPermissionByIdAsync("unknown");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreatePermissionAsync_ReturnsCreatedDto()
        {
            // Arrange
            var request = new PermissionCreateRequest { Name = "New", Description = "Test" };
            var created = new Permission { Id = "1", Name = "New", Description = "Test" };
            _repoMock
                .Setup(r => r.InsertOrUpdatePermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            // Act
            var result = await _service.CreatePermissionAsync(request);

            // Assert
            Assert.Equal("New", result.Name);
            _repoMock.Verify(r => r.InsertOrUpdatePermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePermissionAsync_Throws_WhenRepositoryReturnsNull()
        {
            // Arrange
            var request = new PermissionCreateRequest { Name = "Bad" };
            _repoMock
                .Setup(r => r.InsertOrUpdatePermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Permission)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreatePermissionAsync(request));
        }

        [Fact]
        public async Task UpdatePermissionAsync_UpdatesAndReturnsDto()
        {
            // Arrange
            var existing = new Permission { Id = "1", Name = "Old" };
            _repoMock
                .Setup(r => r.SearchPermissionsAsync(It.Is<PermissionSearchOptions>(o => o.Id == "1"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Permission> { Items = new List<Permission> { existing }, TotalCount = 1 });

            _repoMock
                .Setup(r => r.InsertOrUpdatePermissionAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Permission p, CancellationToken _) => p);

            var request = new PermissionUpdateRequest { Name = "Updated" };

            // Act
            var result = await _service.UpdatePermissionAsync("1", request);

            // Assert
            Assert.Equal("Updated", result.Name);
        }

        [Fact]
        public async Task UpdatePermissionAsync_Throws_WhenNotFound()
        {
            // Arrange
            _repoMock
                .Setup(r => r.SearchPermissionsAsync(It.IsAny<PermissionSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Permission> { Items = new List<Permission>(), TotalCount = 0 });

            var request = new PermissionUpdateRequest { Name = "X" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdatePermissionAsync("nope", request));
        }

        [Fact]
        public async Task DeletePermissionAsync_CallsRepository()
        {
            // Act
            await _service.DeletePermissionAsync("del-id");

            // Assert
            _repoMock.Verify(r => r.DeletePermissionAsync("del-id", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPermissionsByRolesAsync_ReturnsEmpty_WhenRolesNull()
        {
            // Act
            var result = await _service.GetPermissionsByRolesAsync(null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPermissionsByRolesAsync_ReturnsDtos_WhenRolesProvided()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                new Permission { Id = "1", Name = "RolePermission" }
            };
            _repoMock
                .Setup(r => r.SearchPermissionsAsync(It.Is<PermissionSearchOptions>(o => o.Roles.Contains("Admin")), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Permission> { Items = permissions, TotalCount = permissions.Count });

            // Act
            var result = await _service.GetPermissionsByRolesAsync(new List<string> { "Admin" });

            // Assert
            Assert.Single(result);
            Assert.Equal("RolePermission", result[0].Name);
        }
    }
}
