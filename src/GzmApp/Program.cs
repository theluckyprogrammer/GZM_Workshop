using System.Text.Encodings.Web;
using System.Text.Unicode;
using GzmApp;
using GzmApp.Ai;
using GzmApp.Data;
using GzmApp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Sol6:Endpoint and Sol6:ApiKey of the AI model – a local file copied by the teacher, ignored by git.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
var connectionString = GzmConnection.Resolve(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddErrorDescriber<PolishIdentityErrors>();

builder.Services.AddSingleton(sp => new SqlRunner(connectionString, sp.GetRequiredService<ILoggerFactory>()));
builder.Services.AddSingleton<DatabaseBootstrapper>();
builder.Services.AddSol6(builder.Configuration);
// Polish letters stay letters in the HTML (F12 → Response), not codes like &#x105;.
builder.Services.AddWebEncoders(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
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

app.UseStaticFiles();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
