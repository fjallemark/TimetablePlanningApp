using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tellurian.Trains.Planning.Repositories.Access;
using Microsoft.AspNetCore.Localization;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Server.Services;

namespace Tellurian.Trains.Planning.App.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RepositoryOptions>(Configuration.GetSection(nameof(RepositoryOptions)));
            services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));
            services.AddSingleton<IPrintedReportsStore, AccessPrintedReportsStore>();
            services.AddSingleton<PrintedReportsService>();
            services.AddLocalization();
            services.AddControllersWithViews().AddJsonOptions(options =>
                options.JsonSerializerOptions.IgnoreReadOnlyProperties = true
            );
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRequestLocalization(options =>
            {
                options.AddSupportedCultures(Program.SupportedLanguages);
                options.AddSupportedUICultures(Program.SupportedLanguages);
                options.DefaultRequestCulture = new RequestCulture("en");
                options.FallBackToParentCultures = true;
                options.FallBackToParentUICultures = true;
                }
            );
            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
                //endpoints.MapFallbackToController("/api/*", "", "Error");
            });
        }
    }
}
