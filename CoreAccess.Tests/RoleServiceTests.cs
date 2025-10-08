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
    public class RoleServiceTests
    {
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _permissionRepositoryMock = new Mock<IPermissionRepository>();
            _roleService = new RoleService(_roleRepositoryMock.Object, _permissionRepositoryMock.Object);
        }

        [Fact]
        public async Task SearchRolesAsync_ReturnsPagedResult()
        {
            // Arrange
            var roles = new List<Role>
            {
                new Role { Id = Guid.NewGuid().ToString(), Name = "Admin" },
                new Role { Id = Guid.NewGuid().ToString(), Name = "User" }
            };
            _roleRepositoryMock
                .Setup(r => r.SearchRolesAsync(It.IsAny<RoleSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Role> { Items = roles, TotalCount = roles.Count });

            // Act
            var result = await _roleService.SearchRolesAsync(new RoleSearchOptions { Page = 1, PageSize = 10 });

            // Assert
            Assert.Equal(2, result.Items.Count());
            Assert.Contains(result.Items, r => r.Name == "Admin");
        }

        [Fact]
        public async Task GetRoleByIdAsync_ReturnsDetailDto_WhenFound()
        {
            // Arrange
            var roleId = Guid.NewGuid().ToString();
            var role = new Role { Id = roleId, Name = "Manager" };
            _roleRepositoryMock
                .Setup(r => r.SearchRolesAsync(It.Is<RoleSearchOptions>(o => o.Id == roleId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Role> { Items = new List<Role> { role }, TotalCount = 1 });

            // Act
            var result = await _roleService.GetRoleByIdAsync(roleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Manager", result.Name);
        }

        [Fact]
        public async Task GetRoleByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _roleRepositoryMock
                .Setup(r => r.SearchRolesAsync(It.IsAny<RoleSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Role> { Items = new List<Role>(), TotalCount = 0 });

            // Act
            var result = await _roleService.GetRoleByIdAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateRoleAsync_ReturnsCreatedRole()
        {
            // Arrange
            var request = new RoleCreateRequest { Name = "Tester", Description = "Testing role" };
            var role = new Role { Id = Guid.NewGuid().ToString(), Name = request.Name, Description = request.Description };
            _roleRepositoryMock
                .Setup(r => r.InsertOrUpdateRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            // Act
            var result = await _roleService.CreateRoleAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Tester", result.Name);
        }

        [Fact]
        public async Task UpdateRoleAsync_Throws_WhenRoleNotFound()
        {
            // Arrange
            _roleRepositoryMock
                .Setup(r => r.SearchRolesAsync(It.IsAny<RoleSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Role> { Items = new List<Role>(), TotalCount = 0 });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _roleService.UpdateRoleAsync(Guid.NewGuid().ToString(), new RoleUpdateRequest()));
        }

        [Fact]
        public async Task AddPermissionToRoleAsync_AddsPermission_WhenValid()
        {
            // Arrange
            var roleId = Guid.NewGuid().ToString();
            var permissionId = Guid.NewGuid().ToString();

            var role = new Role { Id = roleId, Name = "Admin", Permissions = new List<Permission>() };
            var permission = new Permission { Id = permissionId, Name = "CanEdit" };

            _roleRepositoryMock
                .Setup(r => r.SearchRolesAsync(It.IsAny<RoleSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Role> { Items = new List<Role> { role }, TotalCount = 1 });
            _permissionRepositoryMock
                .Setup(p => p.SearchPermissionsAsync(It.IsAny<PermissionSearchOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Permission> { Items = new List<Permission> { permission }, TotalCount = 1 });
            _roleRepositoryMock
                .Setup(r => r.InsertOrUpdateRoleAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            // Act
            var result = await _roleService.AddPermissionToRoleAsync(roleId, permissionId);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result.Permissions, p => p.Name == "CanEdit");
        }

        [Fact]
        public async Task DeleteRoleAsync_CallsRepository()
        {
           // Arrange
           var roleId = Guid.NewGuid().ToString();
           var role = new Role { Id = roleId, Name = "Sales", Permissions = new List<Permission>() };
           _roleRepositoryMock
               .Setup(r => r.SearchRolesAsync(It.IsAny<RoleSearchOptions>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new PagedResult<Role> { Items = new List<Role> { role }, TotalCount = 1 });

           // Act
           await _roleService.DeleteRoleAsync(roleId);

           // Assert
           _roleRepositoryMock.Verify(r => r.DeleteRoleAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
