using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Localization;
using Tellurian.Trains.Planning.App.Contracts;
using Tellurian.Trains.Planning.App.Server.Services;
using Tellurian.Trains.Planning.Repositories.Access;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RepositoryOptions>(builder.Configuration.GetSection(nameof(RepositoryOptions)));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));
builder.Services.AddSingleton<IPrintedReportsStore, AccessPrintedReportsStore>();
builder.Services.AddSingleton<PrintedReportsService>();
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
    options.JsonSerializerOptions.IgnoreReadOnlyProperties = true
);
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
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
    options.AddSupportedCultures(LanguageService.SupportedLanguages);
    options.AddSupportedUICultures(LanguageService.SupportedLanguages);
    options.DefaultRequestCulture = new RequestCulture("en");
    options.FallBackToParentCultures = true;
    options.FallBackToParentUICultures = true;
}
);
app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

