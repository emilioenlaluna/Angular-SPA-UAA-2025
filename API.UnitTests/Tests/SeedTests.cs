using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data;
using API.DataEntities;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace API.UnitTests.Data
{
    public class SeedTests : IDisposable
    {
        private readonly DataContext _context;
        private readonly Mock<IFileReader> _fileReaderMock;

        public SeedTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _fileReaderMock = new Mock<IFileReader>();
        }

        [Fact]
        public async Task SeedUsersAsync_ShouldReturn_WhenUsersExist()
        {
            _context.Users.Add(new AppUser
            {
                UserName = "existinguser",
                KnownAs = "Existing User",
                Gender = "male",
                City = "CityX",
                Country = "CountryX"
            });
            await _context.SaveChangesAsync();

            await Seed.SeedUsersAsync(_context, _fileReaderMock.Object);

            _fileReaderMock.Verify(fr => fr.ReadAllTextAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SeedUsersAsync_ShouldAddUsers_WhenNoUsersExist()
        {
            // Arrange
            var userDataJson = @"
            [
                {
                    ""UserName"": ""user1"",
                    ""KnownAs"": ""User One"",
                    ""Gender"": ""male"",
                    ""City"": ""City1"",
                    ""Country"": ""Country1""
                },
                {
                    ""UserName"": ""user2"",
                    ""KnownAs"": ""User Two"",
                    ""Gender"": ""female"",
                    ""City"": ""City2"",
                    ""Country"": ""Country2""
                }
            ]";

            _fileReaderMock.Setup(fr => fr.ReadAllTextAsync(It.IsAny<string>()))
                           .ReturnsAsync(userDataJson);

            // Act
            await Seed.SeedUsersAsync(_context, _fileReaderMock.Object);

            // Assert
            var users = await _context.Users.ToListAsync();
            Assert.Equal(2, users.Count);
            Assert.Contains(users, u => u.UserName == "user1");
            Assert.Contains(users, u => u.UserName == "user2");
        }

        [Fact]
        public async Task SeedUsersAsync_ShouldHandleNullUsers_WhenDeserializationFails()
        {
            // Arrange
            _fileReaderMock.Setup(fr => fr.ReadAllTextAsync(It.IsAny<string>()))
                           .ReturnsAsync("Invalid JSON");

            // Act
            await Seed.SeedUsersAsync(_context, _fileReaderMock.Object);

            // Assert
            var users = await _context.Users.ToListAsync();
            Assert.Empty(users);
        }

        [Fact]
        public async Task SeedUsersAsync_ShouldHandleEmptyUserList()
        {
            // Arrange
            _fileReaderMock.Setup(fr => fr.ReadAllTextAsync(It.IsAny<string>()))
                           .ReturnsAsync("[]");

            // Act
            await Seed.SeedUsersAsync(_context, _fileReaderMock.Object);

            // Assert
            var users = await _context.Users.ToListAsync();
            Assert.Empty(users);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
