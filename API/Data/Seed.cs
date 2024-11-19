using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.DataEntities;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        // Sobrecarga sin parámetro IFileReader
        public static async Task SeedUsersAsync(DataContext context)
        {
            await SeedUsersAsync(context, new FileReader());
        }

        // Método original con IFileReader
        public static async Task SeedUsersAsync(DataContext context, IFileReader fileReader)
        {
            if (await context.Users.AnyAsync())
            {
                return;
            }

            var userData = await fileReader.ReadAllTextAsync("Data/UserSeedData.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            List<AppUser> users;

            try
            {
                users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);
            }
            catch (JsonException)
            {
                // Manejar el error de deserialización
                return;
            }

            if (users == null)
            {
                return;
            }

            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLowerInvariant();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("123456"));
                user.PasswordSalt = hmac.Key;

                context.Users.Add(user);
            }

            await context.SaveChangesAsync();
        }
    }
}
