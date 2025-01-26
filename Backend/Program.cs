using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "createadmin")
        {
            await RunAdminCreatorAsync();
        }
        else
        {
            CreateHostBuilder(args).Build().Run();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

    private static async Task RunAdminCreatorAsync()
    {
        var adminCreatorProgramType = typeof(AdminCreator.Program);
        await (Task) adminCreatorProgramType.GetMethod("Main").Invoke(null, new object[] { new string[0] });
    }
}
