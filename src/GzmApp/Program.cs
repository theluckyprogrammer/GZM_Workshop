using System.Text;
using GzmApp;
using GzmApp.Ai;
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
var paths = ImportPaths.Resolve(builder.Environment.ContentRootPath, builder.Configuration["Scenario"] ?? "s01");
var mapping = MappingLoader.Load(paths.MappingFile);
builder.Configuration["DataAttribution"] = mapping.Attribution;
builder.Services.AddSingleton(paths);
builder.Services.AddSingleton(mapping);
builder.Services.AddSingleton<ICsvImporter, CsvImporter>();
builder.Services.AddSingleton<IRecordRepository, RecordRepository>();
builder.Services.AddSingleton<IFavoritesRepository, FavoritesRepository>();
builder.Services.Configure<Sol6Options>(builder.Configuration.GetSection("Sol6"));
builder.Services.AddSingleton<IAiSummarizer, AiSummarizer>();
builder.Services.AddRazorPages();

var app = builder.Build();

await app.Services.GetRequiredService<DatabaseBootstrapper>().RunAsync();

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

app.UseHtmxErrorFragments();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
