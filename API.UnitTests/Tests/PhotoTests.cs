using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using API.Data;
using API.DataEntities;

namespace API.UnitTests.DataEntities
{
    public class PhotoTests : IDisposable
    {
        private readonly DataContext _context;

        public PhotoTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
        }

        [Fact]
        public void Photo_Should_Create_Instance_With_Valid_Data()
        {
            // Arrange
            var appUser = new AppUser
            {
                Id = 1,
                UserName = "testuser",
                KnownAs = "Test User",
                Gender = "male",
                City = "Test City",
                Country = "Test Country"
            };

            var photo = new Photo
            {
                Id = 1,
                Url = "https://example.com/photo.jpg",
                IsMain = true,
                PublicId = "public_id_123",
                AppUserId = appUser.Id,
                AppUser = appUser
            };

            // Act & Assert
            photo.Should().NotBeNull();
            photo.Id.Should().Be(1);
            photo.Url.Should().Be("https://example.com/photo.jpg");
            photo.IsMain.Should().BeTrue();
            photo.PublicId.Should().Be("public_id_123");
            photo.AppUserId.Should().Be(appUser.Id);
            photo.AppUser.Should().Be(appUser);
        }

        [Fact]
        public void Photo_Should_Have_Default_Values_For_Optional_Properties()
        {
            // Arrange
            var appUser = new AppUser
            {
                UserName = "testuser",
                KnownAs = "Test User",
                Gender = "male",
                City = "Test City",
                Country = "Test Country"
            };

            var photo = new Photo
            {
                Url = "https://example.com/photo.jpg",
                AppUserId = 1,
                AppUser = appUser
            };

            // Act & Assert
            photo.Id.Should().Be(0); // Valor predeterminado de int
            photo.IsMain.Should().BeFalse(); // Valor predeterminado de bool
            photo.PublicId.Should().BeNull(); // Propiedad nullable
        }

        [Fact]
        public void Photo_Should_Allow_Property_Updates()
        {
            // Arrange
            var appUser = new AppUser
            {
                UserName = "testuser",
                KnownAs = "Test User",
                Gender = "male",
                City = "Test City",
                Country = "Test Country"
            };

            var photo = new Photo
            {
                Url = "https://example.com/photo.jpg",
                IsMain = false,
                PublicId = "public_id_123",
                AppUserId = 1,
                AppUser = appUser
            };

            // Act
            photo.IsMain = true;
            photo.PublicId = "new_public_id";

            // Assert
            photo.IsMain.Should().BeTrue();
            photo.PublicId.Should().Be("new_public_id");
        }

        [Fact]
        public async Task Photo_Should_Be_Saved_To_Database()
        {
            // Arrange
            var appUser = new AppUser
            {
                Id = 1,
                UserName = "testuser",
                KnownAs = "Test User",
                Gender = "male",
                City = "Test City",
                Country = "Test Country"
            };

            var photo = new Photo
            {
                Url = "https://example.com/photo.jpg",
                IsMain = true,
                PublicId = "public_id_123",
                AppUser = appUser
            };

            // Act
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Assert
            var savedPhoto = await _context.Photos
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync(p => p.Id == photo.Id);

            savedPhoto.Should().NotBeNull();
            savedPhoto.Url.Should().Be("https://example.com/photo.jpg");
            savedPhoto.IsMain.Should().BeTrue();
            savedPhoto.PublicId.Should().Be("public_id_123");
            savedPhoto.AppUser.Should().NotBeNull();
            savedPhoto.AppUser.UserName.Should().Be("testuser");
        }

        [Fact]
        public async Task Photo_Should_Be_Removed_From_Database()
        {
            // Arrange
            var appUser = new AppUser
            {
                UserName = "testuser",
                KnownAs = "Test User",
                Gender = "male",
                City = "Test City",
                Country = "Test Country"
            };

            var photo = new Photo
            {
                Url = "https://example.com/photo.jpg",
                AppUser = appUser
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Act
            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            // Assert
            var deletedPhoto = await _context.Photos.FindAsync(photo.Id);
            deletedPhoto.Should().BeNull();
        }

        [Fact]
        public void Photo_Should_Have_AppUser_Associated()
        {
            // Arrange
            var appUser = new AppUser
            {
                Id = 1,
                UserName = "testuser",
                KnownAs = "Test User",
                Gender = "male",
                City = "Test City",
                Country = "Test Country"
            };

            var photo = new Photo
            {
                Url = "https://example.com/photo.jpg",
                AppUserId = appUser.Id,
                AppUser = appUser
            };

            // Act & Assert
            photo.AppUser.Should().NotBeNull();
            photo.AppUser.Should().Be(appUser);
            photo.AppUserId.Should().Be(appUser.Id);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
