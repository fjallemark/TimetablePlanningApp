using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Tellurian.Trains.Planning.App.Server
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());

        public static string[] SupportedLanguages => new[] { "en", "sv", "da", "nb", "de" };
    }
}
