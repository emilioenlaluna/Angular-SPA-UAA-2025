using System;
using System.Threading.Tasks;
using API.Controllers;
using API.Data;
using API.DTOs;
using API.DataEntities;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;

namespace API.UnitTests.Controllers
{
    public class AccountControllerTests : IDisposable
    {
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly DataContext _context;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            // Configurar el DbContext para usar una base de datos en memoria
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Usar una base de datos única por prueba
                .Options;
            _context = new DataContext(options);

            // Inicializar los mocks
            _tokenServiceMock = new Mock<ITokenService>();

            // Instanciar el controlador con las dependencias mockeadas
            _controller = new AccountController(_context, _tokenServiceMock.Object);
        }

        // Método para limpiar la base de datos después de cada prueba
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        #region RegisterAsync Tests


        [Fact]
        public async Task RegisterAsync_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
        {
            // Arrange
            var existingUser = new AppUser
            {
                UserName = "usuarioExistente",
                PasswordHash = Array.Empty<byte>(),
                PasswordSalt = Array.Empty<byte>(),
                KnownAs = "NombreConocido",
                Gender = "Genero",
                City = "Ciudad",
                Country = "Pais"
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var registerRequest = new RegisterRequest
            {
                Username = "usuarioExistente",
                Password = "Password123!"
            };

            // Act
            var result = await _controller.RegisterAsync(registerRequest);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Username already in use");
        }

        #endregion

        #region LoginAsync Tests


        [Fact]
        public async Task LoginAsync_ShouldReturnUnauthorized_WhenUserDoesNotExist()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "usuarioInexistente",
                Password = "Password123!"
            };

            // Act
            var result = await _controller.LoginAsync(loginRequest);

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
            unauthorizedResult.Value.Should().Be("Invalid username or password");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
        {
            // Arrange
            var password = "Password123!";
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new AppUser
            {
                UserName = "usuarioValido",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                KnownAs = "NombreConocido",
                Gender = "Genero",
                City = "Ciudad",
                Country = "Pais"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequest
            {
                Username = "usuarioValido",
                Password = "ContraseñaIncorrecta"
            };

            // Act
            var result = await _controller.LoginAsync(loginRequest);

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            unauthorizedResult.Should().NotBeNull();
            unauthorizedResult.StatusCode.Should().Be(401);
            unauthorizedResult.Value.Should().Be("Invalid username or password");
        }

        #endregion

        #region Helper Methods

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        #endregion
    }
}
