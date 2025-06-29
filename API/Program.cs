namespace API;

using API.Data;
using API.DataEntities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

[ExcludeFromCodeCoverage]
public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<DataContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManger = services.GetRequiredService<RoleManager<AppRole>>();

            await context.Database.MigrateAsync();
            await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
            await Seed.SeedUsersAsync(userManager, roleManger);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error has ocurred during migration/seeding");
        }

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
