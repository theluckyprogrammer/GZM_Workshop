using System.Text;
using GzmApp;
using GzmApp.Data;
using GzmApp.Data.Import;
using GzmApp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);
var connectionString = GzmConnection.Resolve(builder.Configuration);

builder.Services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppIdentityDbContext>();

builder.Services.AddSingleton(sp => new SqlRunner(connectionString, sp.GetRequiredService<ILoggerFactory>()));
builder.Services.AddSingleton<DatabaseBootstrapper>();
builder.Services.AddSingleton(_ => ImportPaths.Resolve(builder.Environment.ContentRootPath, builder.Configuration["Scenario"] ?? "s01"));
builder.Services.AddSingleton(sp => MappingLoader.Load(sp.GetRequiredService<ImportPaths>().MappingFile));
builder.Services.AddSingleton<ICsvImporter, CsvImporter>();
builder.Services.AddRazorPages();
builder.Services.AddAppServices(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.Services.GetRequiredService<DatabaseBootstrapper>().RunAsync();
await AppServices.RunAtStartupAsync(app.Services);

// "dotnet run -- import": import only (again, idempotent) and exit. Normal start: import when the table is empty.
var importer = app.Services.GetRequiredService<ICsvImporter>();
if (args.Contains("import"))
{
    await importer.ImportAsync();
    return;
}
await importer.ImportIfEmptyAsync();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
