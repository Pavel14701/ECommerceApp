using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace AdminCreator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection, configuration);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var userService = serviceProvider.GetRequiredService<IAdminService>();
            if (userService == null)
            {
                throw new ArgumentNullException(nameof(userService), "UserService cannot be null.");
            }

            await CreateAdminAsync(userService);
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationAdminDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IAdminService, AdminService>();
            services.AddSingleton<IConfiguration>(configuration);
        }

        private static async Task CreateAdminAsync(IAdminService userService)
        {
            Console.Write("Enter username: ");
            var username = Console.ReadLine();
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username), "Username cannot be null or empty.");
            }

            Console.Write("Enter email: ");
            var email = Console.ReadLine();
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
            }

            Console.Write("Enter password: ");
            var password = Console.ReadLine();
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null or empty.");
            }

            var result = await userService.RegisterUser(username, email, password, true);

            if (result.Success)
            {
                Console.WriteLine("Admin created successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to create admin: {result.Message}");
            }
        }
    }
}
