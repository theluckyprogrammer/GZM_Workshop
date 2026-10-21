using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GzmApp.Identity;

// EF Core is allowed only here: tables AspNet* for login. Domain data goes through Dapper (SqlRunner).
public sealed class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext(options);
