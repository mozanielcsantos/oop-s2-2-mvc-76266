using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OopS22Mvc76266.Web.Data;
using OopS22Mvc76266.Web.Data.Seeding;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FoodSafetyInspectionTracker")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure DbContext with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Global error handling + friendly failures
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// Enrich logs with UserName from HttpContext
app.Use(async (context, next) =>
{
    var userName = context.User?.Identity?.IsAuthenticated == true
        ? context.User.Identity?.Name ?? "UnknownUser"
        : "Anonymous";

    using (LogContext.PushProperty("UserName", userName))
    {
        await next();
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Seed data + create roles + assign admin role
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);

    string[] roles = { "Admin", "Inspector", "Viewer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Log.Information("Role created: {RoleName}", role);
        }
    }

    var adminEmail = "admin@test.com";
    var adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);

        if (createResult.Succeeded)
        {
            Log.Information("Seed admin user created: {Email}", adminEmail);
        }
        else
        {
            Log.Warning("Seed admin user creation failed for {Email}: {Errors}",
                adminEmail,
                string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }
    }

    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

        if (roleResult.Succeeded)
        {
            Log.Information("Admin role assigned to seed user: {Email}", adminEmail);
        }
        else
        {
            Log.Warning("Admin role assignment failed for {Email}: {Errors}",
                adminEmail,
                string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();