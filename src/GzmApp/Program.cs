using GzmApp;
using GzmApp.Data;
using GzmApp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = GzmConnection.Resolve(builder.Configuration);

builder.Services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppIdentityDbContext>();

builder.Services.AddSingleton(sp => new SqlRunner(connectionString, sp.GetRequiredService<ILoggerFactory>()));
builder.Services.AddSingleton<DatabaseBootstrapper>();
builder.Services.AddRazorPages();
builder.Services.AddAppServices(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.Services.GetRequiredService<DatabaseBootstrapper>().RunAsync();
await AppServices.RunAtStartupAsync(app.Services);

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
