namespace CoreAccess.Tests.Controllers;

[TestFixture]
public class UserControllerTests
{
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreAccess.WebAPI.Controllers;
using CoreAccess.WebAPI.Model;
using CoreAccess.WebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace CoreAccess.Tests.Controllers
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserService> _userServiceMock;
        private UserController _userController;

        [SetUp]
        public void SetUp()
        {
            _userServiceMock = new Mock<IUserService>();
            _userController = new UserController(_userServiceMock.Object);
        }

        [Test]
        public async Task GetUser_ReturnsOkResult_WithUserList()
        {
            // Arrange
            var options = new CoreUserSearchOptions { Page = 1, PageSize = 10 };
            var users = new PagedResult<CoreUserDto>
            {
                Items = new List<CoreUserDto>
                {
                    new CoreUserDto { Id = Guid.NewGuid(), Username = "testuser1" },
                    new CoreUserDto { Id = Guid.NewGuid(), Username = "testuser2" }
                },
                TotalCount = 2,
                Page = 1,
                PageSize = 10
            };

            _userServiceMock
                .Setup(s => s.SearchUsersAsync(options, It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            // Act
            var result = await _userController.GetUser(options, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOf<PagedResult<CoreUserDto>>(okResult.Value);
            var returnedUsers = okResult.Value as PagedResult<CoreUserDto>;
            Assert.AreEqual(2, returnedUsers.Items.Count);
        }
    }
}