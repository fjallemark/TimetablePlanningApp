using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tellurian.Trains.Planning.App.Client.Services;

namespace Tellurian.Trains.Planning.App.Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        //builder.Services.Configure<AppSettings>(
        //    builder.Configuration.GetSection("AppSettings"));
        builder.Services.AddLocalization();
        builder.Services.AddScoped<LanguageService>();
        builder.Services.AddScoped<PrintedReportsService>();
        builder.Services.AddHttpClient<LanguageService>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
        builder.Services.AddHttpClient<PrintedReportsService>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
        await builder.Build().RunAsync().ConfigureAwait(false);
    }
}
