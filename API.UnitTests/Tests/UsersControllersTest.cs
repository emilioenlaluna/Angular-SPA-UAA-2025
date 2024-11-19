using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Controllers;
using API.Data;
using API.DataEntities;
using API.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace API.UnitTests.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();
            _controller = new UsersController(_userRepositoryMock.Object, _mapperMock.Object);

            // Configurar el contexto de usuario para pruebas con autenticación
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testuser")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ShouldReturnOkResultWithListOfMembers()
        {
            // Arrange
            var members = new List<MemberResponse>
            {
                new MemberResponse { UserName = "User1", Gender = "male" },
                new MemberResponse { UserName = "User2", Gender = "female" }
            };

            _userRepositoryMock.Setup(repo => repo.GetMembersAsync())
                               .ReturnsAsync(members);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnMembers = Assert.IsType<List<MemberResponse>>(okResult.Value);
            Assert.Equal(2, returnMembers.Count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOkResultWithEmptyList()
        {
            // Arrange
            var members = new List<MemberResponse>();

            _userRepositoryMock.Setup(repo => repo.GetMembersAsync())
                               .ReturnsAsync(members);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnMembers = Assert.IsType<List<MemberResponse>>(okResult.Value);
            Assert.Empty(returnMembers);
        }

        [Fact]
        public async Task GetAllAsync_ExceptionThrown_ReturnsStatusCode500()
        {
            // Arrange
            _userRepositoryMock.Setup(repo => repo.GetMembersAsync())
                               .ThrowsAsync(new InvalidOperationException("Database failure"));

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("An error occurred while processing your request.", objectResult.Value);
        }

        #endregion

        #region GetByUsernameAsync Tests

        [Fact]
        public async Task GetByUsernameAsync_ExistingUsername_ReturnsMember()
        {
            // Arrange
            var username = "User1";
            var member = new MemberResponse { UserName = username, Gender = "male" };

            _userRepositoryMock.Setup(repo => repo.GetMemberAsync(username))
                               .ReturnsAsync(member);

            // Act
            var result = await _controller.GetByUsernameAsync(username);

            // Assert
            var actionResult = Assert.IsType<ActionResult<MemberResponse>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnMember = Assert.IsType<MemberResponse>(okResult.Value);
            Assert.Equal(username, returnMember.UserName);
        }

        [Fact]
        public async Task GetByUsernameAsync_NonExistingUsername_ReturnsNotFound()
        {
            // Arrange
            var username = "NonExistentUser";

            _userRepositoryMock.Setup(repo => repo.GetMemberAsync(username))
                               .ReturnsAsync((MemberResponse)null);

            // Act
            var result = await _controller.GetByUsernameAsync(username);

            // Assert
            var actionResult = Assert.IsType<ActionResult<MemberResponse>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetByUsernameAsync_ExceptionThrown_ReturnsStatusCode500()
        {
            // Arrange
            var username = "User1";

            _userRepositoryMock.Setup(repo => repo.GetMemberAsync(username))
                               .ThrowsAsync(new InvalidOperationException("Database failure"));

            // Act
            var result = await _controller.GetByUsernameAsync(username);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.Equal("An error occurred while processing your request.", objectResult.Value);
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_ValidRequest_ReturnsNoContent()
        {
            // Arrange
            var username = "testuser";
            var user = new AppUser
            {
                UserName = username,
                KnownAs = "TestKnownAs",
                Gender = "male",
                City = "TestCity",
                Country = "TestCountry"
            };
            var updateRequest = new MemberUpdateRequest { Introduction = "Updated Introduction" };

            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(username))
                               .ReturnsAsync(user);

            _mapperMock.Setup(m => m.Map(updateRequest, user));

            _userRepositoryMock.Setup(repo => repo.SaveAllAsync())
                               .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateUser(updateRequest);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mapperMock.Verify(m => m.Map(updateRequest, user), Times.Once);
            _userRepositoryMock.Verify(repo => repo.Update(user), Times.Once);
            _userRepositoryMock.Verify(repo => repo.SaveAllAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_NoUsernameInToken_ReturnsBadRequest()
        {
            // Arrange
            var updateRequest = new MemberUpdateRequest { Introduction = "Updated Introduction" };

            // Configurar el User sin el Claim NameIdentifier
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                // No se agrega ClaimTypes.NameIdentifier
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.UpdateUser(updateRequest);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No username found in token", badRequest.Value);
        }

        [Fact]
        public async Task UpdateUser_UserNotFound_ReturnsBadRequest()
        {
            // Arrange
            var username = "testuser";
            var updateRequest = new MemberUpdateRequest { Introduction = "Updated Introduction" };

            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(username))
                               .ReturnsAsync((AppUser)null);

            // Act
            var result = await _controller.UpdateUser(updateRequest);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Could not find user", badRequest.Value);
        }

        [Fact]
        public async Task UpdateUser_SaveFails_ReturnsBadRequest()
        {
            // Arrange
            var username = "testuser";
            var user = new AppUser
            {
                UserName = username,
                KnownAs = "TestKnownAs",
                Gender = "male",
                City = "TestCity",
                Country = "TestCountry"
            };
            var updateRequest = new MemberUpdateRequest { Introduction = "Updated Introduction" };

            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(username))
                               .ReturnsAsync(user);

            _mapperMock.Setup(m => m.Map(updateRequest, user));

            _userRepositoryMock.Setup(repo => repo.SaveAllAsync())
                               .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateUser(updateRequest);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Update user failed!", badRequest.Value);
        }

        [Fact]
        public async Task UpdateUser_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var updateRequest = new MemberUpdateRequest { Introduction = "" }; // Supongamos que Introduction no puede estar vacío

            _controller.ModelState.AddModelError("Introduction", "Introduction is required");

            // Act
            var result = await _controller.UpdateUser(updateRequest);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var serializableError = Assert.IsType<SerializableError>(badRequest.Value);
            Assert.True(serializableError.ContainsKey("Introduction"));
        }

        #endregion
    }
}
