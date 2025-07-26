using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using Ribosoft.Controllers;
using Ribosoft.Data;
using Ribosoft.Models;
using Ribosoft.Models.AdminViewModels;
using Xunit;

namespace Ribosoft.Tests.Controllers
{
    // Helper classes for async enumeration in tests
    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression)!;
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression)!;
        }

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult);
            
            // Handle Task<int> for CountAsync()
            if (expectedResultType == typeof(Task<int>))
            {
                var executionResult = Execute(expression);
                
                // If the result is already an int (from Count()), use it directly
                if (executionResult is int count)
                {
                    return (TResult)(object)Task.FromResult(count);
                }
                
                // Otherwise, treat it as an enumerable and count it
                var enumerable = (IEnumerable<TEntity>)executionResult;
                return (TResult)(object)Task.FromResult(enumerable.Count());
            }
            
            // Handle Task<List<T>> for ToListAsync()
            if (expectedResultType.IsGenericType && expectedResultType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var innerType = expectedResultType.GetGenericArguments()[0];
                if (innerType.IsGenericType && innerType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var executionResult = Execute(expression);
                    var listMethod = typeof(Enumerable).GetMethod("ToList")!.MakeGenericMethod(innerType.GetGenericArguments()[0]);
                    var list = listMethod.Invoke(null, new[] { executionResult });
                    var taskFromResultMethod = typeof(Task).GetMethod("FromResult")!.MakeGenericMethod(innerType);
                    var task = taskFromResultMethod.Invoke(null, new[] { list });
                    return (TResult)task!;
                }
            }

            // Fallback to synchronous execution
            return _inner.Execute<TResult>(expression)!;
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
    /*! \class AdminControllerTests
     * \brief Unit tests for AdminController
     */
    public class AdminControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<ILogger<AdminController>> _loggerMock;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Setup UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            // Setup RoleManager mock
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null!, null!, null!, null!);
            
            // Setup roles for the role manager
            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = "Administrator" },
                new IdentityRole { Name = "User" }
            };
            var mockRoles = new TestAsyncEnumerable<IdentityRole>(roles);
            _roleManagerMock.Setup(rm => rm.Roles).Returns(mockRoles);

            // Add test jobs to the context
            _context.Jobs.AddRange(
                new Job { Id = 1, OwnerId = "1", CreatedAt = DateTime.UtcNow },
                new Job { Id = 2, OwnerId = "1", CreatedAt = DateTime.UtcNow },
                new Job { Id = 3, OwnerId = "2", CreatedAt = DateTime.UtcNow }
            );
            _context.SaveChanges();

            // Setup Logger mock
            _loggerMock = new Mock<ILogger<AdminController>>();

            // Create controller
            _controller = new AdminController(
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _context,
                _loggerMock.Object);

            // Setup controller context with admin user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin@test.com"),
                new Claim(ClaimTypes.Role, "Administrator")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };
            
            // Setup TempData
            var tempDataProvider = new Mock<ITempDataProvider>();
            var tempDataDictionary = new TempDataDictionary(httpContext, tempDataProvider.Object);
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = tempDataDictionary;
        }

        [Fact]
        public async Task Index_ReturnsViewWithUserList()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", UserName = "user1", Email = "user1@test.com" },
                new ApplicationUser { Id = "2", UserName = "user2", Email = "user2@test.com" }
            };

            // Create async queryable mock
            var mockUsers = new TestAsyncEnumerable<ApplicationUser>(users);
            _userManagerMock.Setup(x => x.Users).Returns(mockUsers);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserManagementViewModel>(viewResult.Model);
            Assert.Equal(2, model.Users.Count);
            Assert.Contains("Administrator", model.AvailableRoles);
            Assert.Contains("User", model.AvailableRoles);
        }

        [Fact]
        public async Task Index_WithSearchTerm_FiltersUsers()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", UserName = "john", Email = "john@test.com" },
                new ApplicationUser { Id = "2", UserName = "jane", Email = "jane@test.com" }
            };

            // Create async queryable mock
            var mockUsers = new TestAsyncEnumerable<ApplicationUser>(users);
            _userManagerMock.Setup(x => x.Users).Returns(mockUsers);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.Index("john");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserManagementViewModel>(viewResult.Model);
            Assert.Single(model.Users);
            Assert.Equal("john", model.Users.First().UserName);
        }

        [Fact]
        public async Task GetUserDetails_WithValidId_ReturnsUserDetails()
        {
            // Arrange
            var user = new ApplicationUser 
            { 
                Id = "1", 
                UserName = "testuser", 
                Email = "test@test.com"
            };

            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _controller.GetUserDetails("1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var userDetails = Assert.IsType<UserDetailsViewModel>(jsonResult.Value);
            Assert.Equal("1", userDetails.Id);
            Assert.Equal("testuser", userDetails.UserName);
            Assert.Equal("test@test.com", userDetails.Email);
        }

        [Fact]
        public async Task GetUserDetails_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByIdAsync("invalid")).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.GetUserDetails("invalid");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUserRoles_WithValidData_UpdatesRoles()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", UserName = "testuser" };
            var model = new UpdateUserRolesViewModel
            {
                UserId = "1",
                Roles = new List<string> { "User", "Administrator" }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.UpdateUserRoles(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = jsonResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task UpdateUserRoles_RemovingLastAdmin_ReturnsBadRequest()
        {
            // Arrange
            var adminUser = new ApplicationUser { Id = "1", UserName = "admin" };
            var model = new UpdateUserRolesViewModel
            {
                UserId = "1",
                Roles = new List<string> { "User" } // Removing Administrator role
            };

            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.GetRolesAsync(adminUser)).ReturnsAsync(new List<string> { "Administrator" });
            
            // Setup to return only one admin user
            var users = new List<ApplicationUser> { adminUser };
            _userManagerMock.Setup(x => x.Users).Returns(users.AsQueryable());

            // Act
            var result = await _controller.UpdateUserRoles(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Cannot remove Administrator role from the last administrator", badRequestResult.Value);
        }

        [Fact]
        public async Task ToggleUserLockout_WithRegularUser_TogglesLockout()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", UserName = "testuser" };
            
            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(x => x.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ToggleUserLockout("1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = jsonResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ToggleUserLockout_WithAdministrator_ReturnsBadRequest()
        {
            // Arrange
            var adminUser = new ApplicationUser { Id = "1", UserName = "admin" };
            
            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(adminUser);
            _userManagerMock.Setup(x => x.GetRolesAsync(adminUser)).ReturnsAsync(new List<string> { "Administrator" });

            // Act
            var result = await _controller.ToggleUserLockout("1");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Cannot lock out administrator accounts", badRequestResult.Value);
        }

        [Fact]
        public async Task ResetUserPassword_WithValidUser_ResetsPassword()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", UserName = "testuser" };
            
            _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            _userManagerMock.Setup(x => x.ResetPasswordAsync(user, "reset-token", It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ResetUserPassword("1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = jsonResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ResetUserPassword_WithInvalidUser_ReturnsJsonError()
        {
            // Arrange
            _userManagerMock.Setup(x => x.FindByIdAsync("invalid")).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.ResetUserPassword("invalid");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value;
            Assert.NotNull(value);
            
            // Use reflection to check the anonymous object properties
            var successProperty = value.GetType().GetProperty("success");
            var messageProperty = value.GetType().GetProperty("message");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(messageProperty);
            Assert.False((bool)successProperty.GetValue(value)!);
            Assert.Equal("User not found", (string)messageProperty.GetValue(value)!);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
